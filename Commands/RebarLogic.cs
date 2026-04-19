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
        // Đổi tên hàm thành ExecuteDrawRebar để khớp với lời gọi từ ViewModel của bạn
        public static void ExecuteDrawRebar(ExternalCommandData commandData, RebarViewModel vm)
        {
            Document doc = commandData.Application.ActiveUIDocument.Document;

            try
            {
                // 1. LẤY MÓNG TỪ VIEWMODEL (Sử dụng thuộc tính SelectedHost bạn vừa thêm)
                Element host = vm.SelectedHost;

                if (host == null)
                {
                    TaskDialog.Show("Lỗi", "Không tìm thấy móng đã chọn.");
                    return;
                }

                // 2. LẤY THÔNG TIN HÌNH HỌC
                BoundingBoxXYZ bbox = host.get_BoundingBox(null);
                if (bbox == null) throw new Exception("Không lấy được ranh giới móng.");

                XYZ min = bbox.Min;
                XYZ max = bbox.Max;

                // Tính toán các thông số khoảng cách
                double lengthY = max.Y - min.Y;
                // Sử dụng 50mm lớp bảo vệ theo TCVN
                double offset = UnitUtils.ConvertToInternalUnits(50, UnitTypeId.Millimeters);

                using (Transaction trans = new Transaction(doc, "Vẽ thép móng"))
                {
                    trans.Start();

                    // 3. TÌM LOẠI THÉP (BARTYPE)
                    // Tìm loại thép có tên chứa con số đường kính từ giao diện (vm.LongDiameter)
                    RebarBarType barType = new FilteredElementCollector(doc)
                        .OfClass(typeof(RebarBarType))
                        .Cast<RebarBarType>()
                        .FirstOrDefault(x => x.Name.Contains(vm.LongDiameter.ToString()));

                    // Nếu không tìm thấy, lấy loại thép đầu tiên trong dự án để tránh crash
                    if (barType == null)
                    {
                        barType = new FilteredElementCollector(doc)
                            .OfClass(typeof(RebarBarType))
                            .FirstElement() as RebarBarType;
                    }

                    if (barType == null)
                        throw new Exception("Dự án chưa nạp thư viện thép (Rebar Bar Type).");

                    // 4. ĐỊNH NGHĨA ĐƯỜNG DẪN THANH THÉP (THANH ĐẦU TIÊN)
                    // Vẽ dọc theo phương X, cao độ Z cách đáy móng một khoảng offset
                    XYZ startPoint = new XYZ(min.X + offset, min.Y + offset, min.Z + offset);
                    XYZ endPoint = new XYZ(max.X - offset, min.Y + offset, min.Z + offset);
                    Line line = Line.CreateBound(startPoint, endPoint);

                    // 5. TẠO THANH THÉP
                    // Sử dụng XYZ.BasisY làm Normal vector như bạn mong muốn
                    Rebar rebar = Rebar.CreateFromCurves(
                        doc,
                        RebarStyle.Standard,
                        barType,
                        null,
                        null,
                        host,
                        XYZ.BasisY,
                        new List<Curve> { line },
                        RebarHookOrientation.Right,
                        RebarHookOrientation.Right,
                        true,
                        true);

                    // 6. RẢI THÉP THEO SỐ LƯỢNG (LAYOUT)
                    if (rebar != null)
                    {
                        RebarShapeDrivenAccessor accessor = rebar.GetShapeDrivenAccessor();
                        // Rải theo số lượng thanh LongCount nhập từ UI
                        accessor.SetLayoutAsFixedNumber(
                            vm.LongCount,
                            lengthY - (2 * offset),
                            true, true, true);
                    }

                    trans.Commit();
                }

                TaskDialog.Show("Thành công", $"Đã vẽ xong {vm.LongCount} thanh thép cạnh dài.");
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi Logic", "Lỗi khi vẽ thép: " + ex.Message);
            }
        }
    }
}