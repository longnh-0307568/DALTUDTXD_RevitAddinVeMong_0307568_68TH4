using System;
using System.Linq;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.DB.Structure;

namespace AddinVeMong
{
    [Transaction(TransactionMode.Manual)]
    public class CmdVeMongDon : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            // 1. Lấy Level và FamilySymbol (giống bản cũ)
            Level level = doc.ActiveView.GenLevel ?? new FilteredElementCollector(doc).OfClass(typeof(Level)).Cast<Level>().FirstOrDefault();

            FamilySymbol mongType = new FilteredElementCollector(doc)
                .OfClass(typeof(FamilySymbol))
                .OfCategory(BuiltInCategory.OST_StructuralFoundation)
                .Cast<FamilySymbol>()
                .FirstOrDefault();

            if (mongType == null)
            {
                message = "Vui lòng load Family móng đơn trước!";
                return Result.Failed;
            }

            // 2. Vòng lặp để vẽ liên tục
            bool tiepTuc = true;
            int soLuong = 0;

            while (tiepTuc)
            {
                try
                {
                    // Yêu cầu người dùng click chọn điểm trên màn hình
                    // Nếu nhấn Esc, dòng này sẽ văng ra Exception và nhảy xuống catch để thoát vòng lặp
                    XYZ point = uiDoc.Selection.PickPoint("Click chuột để đặt móng (Nhấn Esc để dừng)");

                    using (Transaction trans = new Transaction(doc, "Vẽ móng liên tục"))
                    {
                        trans.Start();
                        if (!mongType.IsActive) mongType.Activate();

                        doc.Create.NewFamilyInstance(point, mongType, level, StructuralType.Footing);

                        trans.Commit();
                    }
                    soLuong++;
                }
                catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                {
                    // Khi người dùng nhấn Esc, vòng lặp sẽ dừng lại ở đây
                    tiepTuc = false;
                }
                catch (Exception ex)
                {
                    TaskDialog.Show("Lỗi", ex.Message);
                    tiepTuc = false;
                }
            }

            if (soLuong > 0)
            {
                TaskDialog.Show("Thành công", $"Đã vẽ xong {soLuong} móng đơn đúng tâm!");
            }

            return Result.Succeeded;
        }
    }
}