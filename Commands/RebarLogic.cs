using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Commands
{
    public static class RebarLogic
    {
        public static void ExecuteDrawRebar(ExternalCommandData commandData, RebarViewModel vm)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                Element host = vm.SelectedHost;
                if (host == null) return;

                BoundingBoxXYZ bbox = host.get_BoundingBox(null);
                if (bbox == null) return;

                // Lớp bảo vệ 50mm
                double offset = UnitUtils.ConvertToInternalUnits(50, UnitTypeId.Millimeters);

                using (Transaction trans = new Transaction(doc, "Vẽ thép móng"))
                {
                    trans.Start();

                    // 1. VẼ THÉP CẠNH DÀI (Dọc phương X, rải phương Y)
                    Rebar longBar = CreateLongRebar(doc, host, bbox, offset, vm);

                    // 2. VẼ THÉP CẠNH NGẮN (Dọc phương Y, rải phương X)
                    // Thép này sẽ nằm đè lên trên thép dài
                    if (longBar != null)
                    {
                        CreateShortRebar(doc, host, bbox, offset, vm);
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Thành công", "Đã vẽ xong hệ thép móng.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi Logic", "Lỗi: " + ex.Message);
            }
        }

        private static Rebar CreateLongRebar(Document doc, Element host, BoundingBoxXYZ bbox, double offset, RebarViewModel vm)
        {
            RebarBarType barType = GetBarType(doc, vm.LongDiameter);
            if (barType == null) return null;

            double z = bbox.Min.Z + offset;
            XYZ start = new XYZ(bbox.Min.X + offset, bbox.Min.Y + offset, z);
            XYZ end = new XYZ(bbox.Max.X - offset, bbox.Min.Y + offset, z);
            Line line = Line.CreateBound(start, end);

            Rebar rebar = Rebar.CreateFromCurves(doc, RebarStyle.Standard, barType, null, null, host, XYZ.BasisY, new List<Curve> { line }, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);

            if (rebar != null)
            {
                double lengthY = (bbox.Max.Y - bbox.Min.Y) - (2 * offset);
                rebar.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(vm.LongCount, lengthY, true, true, true);
            }
            return rebar;
        }

        private static void CreateShortRebar(Document doc, Element host, BoundingBoxXYZ bbox, double offset, RebarViewModel vm)
        {
            RebarBarType longType = GetBarType(doc, vm.LongDiameter);
            RebarBarType shortType = GetBarType(doc, vm.ShortDiameter);
            if (longType == null || shortType == null) return;

            // Lấy bán kính của cả hai loại thép
            double longRadius = longType.BarModelDiameter / 2.0;
            double shortRadius = shortType.BarModelDiameter / 2.0;

            // Cao độ chuẩn: Tâm thép dài + Bán kính thép dài + Bán kính thép ngắn
            // Như vậy mép ngoài của chúng sẽ tiếp xúc nhau (khoảng cách = 0)
            double z = bbox.Min.Z + offset + longRadius + shortRadius;

            XYZ start = new XYZ(bbox.Min.X + offset, bbox.Min.Y + offset, z);
            XYZ end = new XYZ(bbox.Min.X + offset, bbox.Max.Y - offset, z);
            Line line = Line.CreateBound(start, end);

            // Dùng BasisX làm Normal để rải dọc theo phương X
            Rebar rebar = Rebar.CreateFromCurves(doc, RebarStyle.Standard, shortType, null, null, host, XYZ.BasisX, new List<Curve> { line }, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);

            if (rebar != null)
            {
                double lengthX = (bbox.Max.X - bbox.Min.X) - (2 * offset);
                rebar.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(vm.ShortCount, lengthX, true, true, true);
            }
        }

        private static RebarBarType GetBarType(Document doc, int diameter)
        {
            RebarBarType type = new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .FirstOrDefault(x => x.Name.Contains(diameter.ToString()));

            return type ?? new FilteredElementCollector(doc).OfClass(typeof(RebarBarType)).FirstElement() as RebarBarType;
        }
    }
}