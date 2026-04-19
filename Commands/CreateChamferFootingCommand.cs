using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class CreateChamferFootingCommand : IExternalCommand
    {
        private double _baseSize = 4000;
        private double _topSize = 1200;
        private double _hBase = 300;
        private double _hStraight = 300;
        private double _hChamfer = 500;

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                View view = doc.ActiveView;
                if (!(view is ViewPlan plan))
                {
                    TaskDialog.Show("Lỗi", "Phải chạy trong mặt bằng (Plan View)");
                    return Result.Failed;
                }

                Level level = plan.GenLevel;
                double z = level.Elevation;

                List<XYZ> points = new List<XYZ>();

                // Chọn điểm
                while (true)
                {
                    try
                    {
                        XYZ p = uidoc.Selection.PickPoint("Chọn điểm (ESC để kết thúc)");
                        points.Add(p);
                    }
                    catch
                    {
                        break; // Nhấn ESC để dừng
                    }
                }

                if (points.Count > 0)
                {
                    using (Transaction t = new Transaction(doc, "Create Chamfer Footing"))
                    {
                        t.Start();
                        foreach (XYZ p in points)
                        {
                            XYZ center = new XYZ(p.X, p.Y, z);
                            CreateChamferFooting(doc, center);
                        }
                        t.Commit();
                    }
                    TaskDialog.Show("OK", $"Đã vẽ {points.Count} móng vát!");
                }
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        private void CreateChamferFooting(Document doc, XYZ center)
        {
            // Chuyển đổi sang feet (1 foot = 304.8 mm)
            double bSize = _baseSize / 304.8;
            double tSize = _topSize / 304.8;
            double hB = _hBase / 304.8;
            double hS = _hStraight / 304.8;
            double hC = _hChamfer / 304.8;

            double z0 = center.Z;
            double z1 = z0 + hB;
            double z2 = z1 + hS;
            double z3 = z2 + hC;

            // Khối 1: Đế và đoạn thẳng
            CurveLoop baseLoop = CreateSquare(center, bSize, z0);
            Solid solid1 = GeometryCreationUtilities.CreateExtrusionGeometry(
                new List<CurveLoop> { baseLoop },
                XYZ.BasisZ,
                hB + hS
            );

            // Khối 2: Phần vát (Loft)
            CurveLoop loopBottomChamfer = CreateSquare(center, bSize, z2);
            CurveLoop loopTopChamfer = CreateSquare(center, tSize, z3);

            SolidOptions opt = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);
            Solid solid2 = GeometryCreationUtilities.CreateLoftGeometry(
                new List<CurveLoop> { loopBottomChamfer, loopTopChamfer },
                opt
            );

            List<GeometryObject> solids = new List<GeometryObject>() { solid1, solid2 };
            DirectShape ds = DirectShape.CreateElement(doc, new ElementId(BuiltInCategory.OST_StructuralFoundation));
            ds.SetShape(solids);
        }

        private CurveLoop CreateSquare(XYZ center, double size, double z)
        {
            double half = size / 2;
            XYZ p1 = new XYZ(center.X - half, center.Y - half, z);
            XYZ p2 = new XYZ(center.X + half, center.Y - half, z);
            XYZ p3 = new XYZ(center.X + half, center.Y + half, z);
            XYZ p4 = new XYZ(center.X - half, center.Y + half, z);

            CurveLoop loop = new CurveLoop();
            loop.Append(Line.CreateBound(p1, p2));
            loop.Append(Line.CreateBound(p2, p3));
            loop.Append(Line.CreateBound(p3, p4));
            loop.Append(Line.CreateBound(p4, p1));
            return loop;
        }
    }
}