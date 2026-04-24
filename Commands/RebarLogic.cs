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
                // Lấy danh sách móng từ VM
                List<Element> hosts = vm.SelectedHosts;
                if (hosts == null || hosts.Count == 0) return;

                using (Transaction trans = new Transaction(doc, "Vẽ thép móng hàng loạt"))
                {
                    trans.Start();

                    foreach (Element host in hosts)
                    {
                        BoundingBoxXYZ bbox = host.get_BoundingBox(null);
                        if (bbox == null) continue;

                        double offset = UnitUtils.ConvertToInternalUnits(50, UnitTypeId.Millimeters);

                        // 1. Vẽ thép cạnh dài
                        Rebar longBar = CreateLongRebar(doc, host, bbox, offset, vm);

                        // 2. Vẽ thép cạnh ngắn (Thêm longBar vào tham số thứ 6)
                        if (longBar != null)
                        {
                            CreateShortRebar(doc, host, bbox, offset, vm, longBar);
                        }
                    }

                    trans.Commit();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi", ex.Message);
            }
        }

        private static Rebar CreateLongRebar(Document doc, Element host, BoundingBoxXYZ bbox, double offset, RebarViewModel vm)
        {
            RebarBarType type = GetBarType(doc, vm.LongDiameter);
            if (type == null) return null;

            // 1. Phương của thanh thép: Chạy từ MinX đến MaxX (Dọc trục X)
            // Cao độ z: Đáy móng + Lớp bảo vệ + Bán kính thanh thép
            double z = bbox.Min.Z + offset + (type.BarModelDiameter / 2.0);

            XYZ start = new XYZ(bbox.Min.X + offset, bbox.Min.Y + offset, z);
            XYZ end = new XYZ(bbox.Max.X - offset, bbox.Min.Y + offset, z);
            Line line = Line.CreateBound(start, end);

            // 2. Hướng rải: Rải thép dọc theo phương Y (Normal = BasisY)
            Rebar rebar = Rebar.CreateFromCurves(doc, RebarStyle.Standard, type, null, null, host, XYZ.BasisY, new List<Curve> { line }, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);

            if (rebar != null)
            {
                double distY = (bbox.Max.Y - bbox.Min.Y) - (2 * offset);
                rebar.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(vm.LongCount, distY, true, true, true);
            }
            return rebar;
        }

        private static void CreateShortRebar(Document doc, Element host, BoundingBoxXYZ bbox, double offset, RebarViewModel vm, Rebar longBar)
        {
            RebarBarType shortType = GetBarType(doc, vm.ShortDiameter);
            if (shortType == null || longBar == null) return;

            // SỬA LỖI CanGetSubstituteRebarBarType:
            // Lấy đường kính thép dài trực tiếp từ Type của nó
            ElementId longTypeId = longBar.GetTypeId();
            RebarBarType longType = doc.GetElement(longTypeId) as RebarBarType;

            double longDiam = (longType != null) ? longType.BarModelDiameter : 0;
            double shortRadius = shortType.BarModelDiameter / 2.0;

            // Cao độ z: Đáy móng + Lớp bảo vệ + Đường kính thép dài + Bán kính thép ngắn
            double z = bbox.Min.Z + offset + longDiam + shortRadius;

            XYZ start = new XYZ(bbox.Min.X + offset, bbox.Min.Y + offset, z);
            XYZ end = new XYZ(bbox.Min.X + offset, bbox.Max.Y - offset, z);
            Line line = Line.CreateBound(start, end);

            // Tạo thép ngắn rải dọc theo phương X (Normal = BasisX)
            Rebar rebar = Rebar.CreateFromCurves(doc, RebarStyle.Standard, shortType, null, null, host, XYZ.BasisX, new List<Curve> { line }, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);

            if (rebar != null)
            {
                double distX = (bbox.Max.X - bbox.Min.X) - (2 * offset);
                rebar.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(vm.ShortCount, distX, true, true, true);
            }
        }

        private static RebarBarType GetBarType(Document doc, int diameter)
        {
            return new FilteredElementCollector(doc)
                .OfClass(typeof(RebarBarType))
                .Cast<RebarBarType>()
                .FirstOrDefault(x => Math.Abs(x.BarModelDiameter - UnitUtils.ConvertToInternalUnits(diameter, UnitTypeId.Millimeters)) < 0.001
                                  || x.Name.Contains(diameter.ToString()));
        }
    }
}