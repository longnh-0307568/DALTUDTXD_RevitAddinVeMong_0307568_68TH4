using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateChamferFooting : IExternalCommand
    {
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

                // ===== PICK NHIỀU ĐIỂM =====
                while (true)
                {
                    try
                    {
                        XYZ p = uidoc.Selection.PickPoint("Chọn điểm (ESC để kết thúc)");
                        points.Add(p);
                    }
                    catch
                    {
                        break; // ESC
                    }
                }

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

                TaskDialog.Show("OK", "Đã vẽ móng vát!");
                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }

        // =============================
        // VẼ MÓNG VÁT
        // =============================
        private void CreateChamferFooting(Document doc, XYZ center)
        {
            double baseSize = 4000 / 304.8;
            double midSize = 2500 / 304.8;
            double topSize = 1200 / 304.8;

            double hBase = 300 / 304.8;
            double hStraight = 300 / 304.8;
            double hChamfer = 500 / 304.8;

            double z0 = center.Z;
            double z1 = z0 + hBase;
            double z2 = z1 + hStraight;
            double z3 = z2 + hChamfer;

            // =========================
            // KHỐI 1: ĐÁY + ĐOẠN ĐỨNG (EXTRUDE)
            // =========================
            CurveLoop baseLoop = CreateSquare(center, baseSize, z0);

            Solid solid1 = GeometryCreationUtilities.CreateExtrusionGeometry(
                new List<CurveLoop> { baseLoop },
                XYZ.BasisZ,
                hBase + hStraight
            );

            // =========================
            // KHỐI 2: PHẦN VÁT (LOFT)
            // =========================
            CurveLoop loopBottomChamfer = CreateSquare(center, baseSize, z2);
            CurveLoop loopTopChamfer = CreateSquare(center, topSize, z3);

            SolidOptions opt = new SolidOptions(ElementId.InvalidElementId, ElementId.InvalidElementId);

            Solid solid2 = GeometryCreationUtilities.CreateLoftGeometry(
                new List<CurveLoop> { loopBottomChamfer, loopTopChamfer },
                opt
            );

            // =========================
            // GỘP SHAPE
            // =========================
            List<GeometryObject> solids = new List<GeometryObject>()
    {
        solid1,
        solid2
    };

            DirectShape ds = DirectShape.CreateElement(doc,
                new ElementId(BuiltInCategory.OST_StructuralFoundation));

            ds.SetShape(solids);
        }

        // =============================
        // TẠO HÌNH VUÔNG
        // =============================
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
