using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateChamferFootingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            return Result.Succeeded;
        }

        public static void ExecuteLogic(ExternalCommandData commandData, ChamferFootingViewModel vm)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                View view = doc.ActiveView;
                if (!(view is ViewPlan plan))
                {
                    TaskDialog.Show("Lỗi", "Phải chạy trong mặt bằng (Plan View)");
                    return;
                }

                Level level = plan.GenLevel;
                double z = level.Elevation;

                // đổi đơn vị (mm -> feet)
                // Đáy móng
                double l = UnitUtils.ConvertToInternalUnits(vm.Length, UnitTypeId.Millimeters);
                double b = UnitUtils.ConvertToInternalUnits(vm.Width, UnitTypeId.Millimeters);

                // Đỉnh móng (phần cổ móng)
                double lTop = UnitUtils.ConvertToInternalUnits(vm.TopLength, UnitTypeId.Millimeters);
                double bTop = UnitUtils.ConvertToInternalUnits(vm.TopWidth, UnitTypeId.Millimeters);

                // Các thông số chiều cao
                double hB = UnitUtils.ConvertToInternalUnits(vm.HBase, UnitTypeId.Millimeters);
                double hS = UnitUtils.ConvertToInternalUnits(vm.HStraight, UnitTypeId.Millimeters);
                double hC = UnitUtils.ConvertToInternalUnits(vm.HChamfer, UnitTypeId.Millimeters);

                while (true)
                {
                    try
                    {
                        XYZ p = uidoc.Selection.PickPoint("Chọn điểm đặt móng (ESC để kết thúc)");

                        using (Transaction trans = new Transaction(doc, "Vẽ móng vát hình chữ nhật"))
                        {
                            trans.Start();
                            // Truyền đầy đủ các thông số dài, rộng vào hàm vẽ
                            CreateFooting(doc, p, l, b, lTop, bTop, hB, hS, hC);
                            trans.Commit();
                        }
                    }
                    catch (Autodesk.Revit.Exceptions.OperationCanceledException)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi", ex.Message);
            }
        }

        private static void CreateFooting(Document doc, XYZ center, double L, double B, double lTop, double bTop, double hB, double hS, double hC)
        {
            double z0 = center.Z;
            double z1 = z0 + hB;
            double z2 = z1 + hS;
            double z3 = z2 + hC;

            // 1. Khối đế và thân thẳng (Extrusion)
            CurveLoop loopBase = CreateRectangle(center, L, B, z0);
            Solid solid1 = GeometryCreationUtilities.CreateExtrusionGeometry(
                new List<CurveLoop> { loopBase },
                XYZ.BasisZ,
                hB + hS
            );

            // 2. Khối vát (Loft từ đáy lớn lên đáy nhỏ)
            CurveLoop loopBottomChamfer = CreateRectangle(center, L, B, z2);
            CurveLoop loopTopChamfer = CreateRectangle(center, lTop, bTop, z3);

            SolidOptions opt = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            Solid solid2 = GeometryCreationUtilities.CreateLoftGeometry(
                new List<CurveLoop> { loopBottomChamfer, loopTopChamfer },
                opt
            );

            List<GeometryObject> solids = new List<GeometryObject>() { solid1, solid2 };

            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_StructuralFoundation));
            ds.SetShape(solids);
        }

        // Hàm hỗ trợ vẽ hình chữ nhật từ tâm với chiều dài và chiều rộng khác nhau
        private static CurveLoop CreateRectangle(XYZ center, double length, double width, double z)
        {
            double halfL = length / 2;
            double halfW = width / 2;

            XYZ p1 = new XYZ(center.X - halfL, center.Y - halfW, z);
            XYZ p2 = new XYZ(center.X + halfL, center.Y - halfW, z);
            XYZ p3 = new XYZ(center.X + halfL, center.Y + halfW, z);
            XYZ p4 = new XYZ(center.X - halfL, center.Y + halfW, z);

            CurveLoop loop = new CurveLoop();
            loop.Append(Line.CreateBound(p1, p2));
            loop.Append(Line.CreateBound(p2, p3));
            loop.Append(Line.CreateBound(p3, p4));
            loop.Append(Line.CreateBound(p4, p1));
            return loop;
        }
    }
}