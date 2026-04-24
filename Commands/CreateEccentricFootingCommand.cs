using AddinVeMong1.Views;
using AddinVeMong1.ViewModels;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Linq;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CmdVeMongLechTam : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            MainWindowView mainView = new MainWindowView(commandData);
            bool? result = mainView.ShowDialog();

            if (result != true || mainView.ViewModel == null || !mainView.ViewModel.IsAccepted)
                return Result.Cancelled;

            MainViewModel vm = mainView.ViewModel;

            if (!double.TryParse(vm.ChieuDai, out double l_mm) ||
                !double.TryParse(vm.ChieuRong, out double b_mm) ||
                !double.TryParse(vm.ChieuCao, out double h_mm) ||
                !double.TryParse(vm.DoLech, out double e_mm))
            {
                message = "Dữ liệu nhập không hợp lệ.";
                return Result.Failed;
            }

            double lechX = UnitUtils.ConvertToInternalUnits(e_mm, UnitTypeId.Millimeters);
            double lechY = 0;

            double l = UnitUtils.ConvertToInternalUnits(l_mm, UnitTypeId.Millimeters);
            double b = UnitUtils.ConvertToInternalUnits(b_mm, UnitTypeId.Millimeters);
            double h = UnitUtils.ConvertToInternalUnits(h_mm, UnitTypeId.Millimeters);

            Level level = null;

            if (doc.ActiveView is ViewPlan viewPlan)
                level = viewPlan.GenLevel;

            if (level == null)
            {
                level = new FilteredElementCollector(doc)
                    .OfClass(typeof(Level))
                    .Cast<Level>()
                    .FirstOrDefault();
            }

            if (level == null)
            {
                message = "Không tìm thấy Level để đặt móng.";
                return Result.Failed;
            }

            FamilySymbol mongType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .Cast<FamilySymbol>()
                .FirstOrDefault();

            if (mongType == null)
            {
                message = "Vui lòng load Family móng đơn trước.";
                return Result.Failed;
            }

            int soLuong = 0;
            bool tiepTuc = true;

            while (tiepTuc)
            {
                try
                {
                    XYZ pointCot = uiDoc.Selection.PickPoint("Click điểm tim cột để đặt móng lệch tâm (Esc để dừng)");

                    XYZ pointMong = new XYZ(
                        pointCot.X + lechX,
                        pointCot.Y + lechY,
                        pointCot.Z
                    );

                    using (Transaction trans = new Transaction(doc, "Vẽ móng đơn lệch tâm"))
                    {
                        trans.Start();

                        if (!mongType.IsActive)
                        {
                            mongType.Activate();
                            doc.Regenerate();
                        }

                        FamilyInstance mong = doc.Create.NewFamilyInstance(
                            pointMong,
                            mongType,
                            level,
                            StructuralType.Footing
                        );

                        SetParamIfExists(mong, "Length", l);
                        SetParamIfExists(mong, "Width", b);
                        SetParamIfExists(mong, "Thickness", h);

                        SetParamIfExists(mong, "L", l);
                        SetParamIfExists(mong, "B", b);
                        SetParamIfExists(mong, "H", h);

                        trans.Commit();
                    }

                    soLuong++;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    tiepTuc = false;
                }
                catch (Exception ex)
                {
                    message = ex.Message;
                    return Result.Failed;
                }
            }

            if (soLuong > 0)
            {
                TaskDialog.Show("Thành công", $"Đã vẽ xong {soLuong} móng đơn lệch tâm.");
            }

            return Result.Succeeded;
        }

        private void SetParamIfExists(FamilyInstance instance, string paramName, double value)
        {
            Parameter p = instance.LookupParameter(paramName);
            if (p != null && !p.IsReadOnly && p.StorageType == StorageType.Double)
            {
                p.Set(value);
            }
        }
    }
}
