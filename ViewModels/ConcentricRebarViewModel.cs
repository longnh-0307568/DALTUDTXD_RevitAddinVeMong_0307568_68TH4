using AddinVeMong.Commands;
using AddinVeMong.Models;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    public class ConcentricRebarViewModel : INotifyPropertyChanged
    {
        private readonly ExternalCommandData _commandData;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly List<Element> _selectedFootings; // Danh sách móng chọn trong Revit

        private FootingRebarModel _rebarData = new FootingRebarModel();
        public FootingRebarModel RebarData
        {
            get => _rebarData;
            set { _rebarData = value; OnPropertyChanged(); }
        }

        public ICommand DrawRebarCommand { get; }

        public ConcentricRebarViewModel(ExternalCommandData commandData, List<Element> footings)
        {
            // Lưu các thông tin từ Revit để sử dụng sau này(Document, UIDocument, danh sách móng)
            _commandData = commandData;
            _doc = commandData.Application.ActiveUIDocument.Document;
            _uiDoc = commandData.Application.ActiveUIDocument;
            _selectedFootings = footings;

            DrawRebarCommand = new RelayCommand(ExecuteDrawRebar);
        }

        // Vẽ thép
        private void ExecuteDrawRebar(object parameter)
        {
            try
            {
                // Kiểm tra đảm bảo người dùng đã chọn móng trước khi nhấn nút, 
                if (_selectedFootings == null || _selectedFootings.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Không tìm thấy dữ liệu móng được chọn.");
                    return;
                }

                View3D activeView3D = _doc.ActiveView as View3D;

                // Bắt đầu một transaction: Mọi thay đổi trong Revit đều phải nằm trong này
                using (Transaction trans = new Transaction(_doc, "Tạo Thép Móng Hàng Loạt"))
                {
                    trans.Start();

                    // Tìm kiếm tất cả các loại thép (RebarBarType)
                    var barTypes = new FilteredElementCollector(_doc)
                        .OfClass(typeof(RebarBarType))
                        .Cast<RebarBarType>()
                        .ToList();

                    if (barTypes.Count == 0)
                    {
                        TaskDialog.Show("Thiếu Dữ Liệu", "Dự án chưa được tải Family Thép.");
                        trans.RollBack(); // Hủy bỏ các thay đổi nếu có
                        return;
                    }

                    // Tham chiếu từ model (RebarData.LongDiameter, RebarData.ShortDiameter,...)
                    RebarBarType longBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.LongDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.LongDiameter) < 0.1);
                    RebarBarType shortBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.ShortDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.ShortDiameter) < 0.1);
                    RebarBarType starterBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.StarterDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.StarterDiameter) < 0.1);
                    RebarBarType stirrupBarType = barTypes.FirstOrDefault(t => t.Name.Contains(RebarData.StirrupDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - RebarData.StirrupDiameter) < 0.1);

                    if (longBarType == null) longBarType = barTypes.FirstOrDefault();
                    if (shortBarType == null) shortBarType = barTypes.FirstOrDefault();
                    if (starterBarType == null) starterBarType = barTypes.FirstOrDefault();
                    if (stirrupBarType == null) stirrupBarType = barTypes.FirstOrDefault();

                    RebarStyle styleStandard = RebarStyle.Standard;
                    RebarStyle styleStirrup = RebarStyle.StirrupTie;

                    //Duyệt qua từng móng được chọn, dùng thuật toán quét không gian (BoundingBox) để tìm cột tương ứng phía trên
                    foreach (Element footingElement in _selectedFootings)
                    {
                        // Kiểm tra kiểu đối tượng
                        if (footingElement is not FamilyInstance footingInstance) continue; // dùng Pattern Matching net8 cho gọn

                        Element columnElement = null;
                        var boundingBox = footingElement.get_BoundingBox(null);
                        if (boundingBox != null)
                        {
                            XYZ tolerance = new XYZ(0.1, 0.1, 2.0);
                            Outline outline = new Outline(boundingBox.Min, boundingBox.Max + tolerance);
                            BoundingBoxIntersectsFilter boxFilter = new BoundingBoxIntersectsFilter(outline);

                            var connectedColumns = new FilteredElementCollector(_doc)
                                .OfCategory(BuiltInCategory.OST_StructuralColumns)
                                .OfClass(typeof(FamilyInstance))
                                .WherePasses(boxFilter)
                                .ToList();

                            if (connectedColumns.Count > 0) columnElement = connectedColumns.First();
                        }

                        Element columnHostElement = columnElement ?? footingElement;

                        ElementType footingType = _doc.GetElement(footingElement.GetTypeId()) as ElementType;
                        if (footingType == null) continue;

                        Parameter pLength = footingType.LookupParameter("Length");
                        Parameter pWidth = footingType.LookupParameter("Width");
                        Parameter pHeight = footingType.LookupParameter("Foundation Thickness") ?? footingType.LookupParameter("Thickness");

                        if (pLength == null || pWidth == null || pHeight == null) continue;

                        double footingLengthMm = UnitUtils.ConvertFromInternalUnits(pLength.AsDouble(), UnitTypeId.Millimeters);
                        double footingWidthMm = UnitUtils.ConvertFromInternalUnits(pWidth.AsDouble(), UnitTypeId.Millimeters);
                        double footingHeightMm = UnitUtils.ConvertFromInternalUnits(pHeight.AsDouble(), UnitTypeId.Millimeters);

                        // Tham chiếu từ model (RebarData.Cover)
                        double coverFoot = UnitUtils.ConvertToInternalUnits(RebarData.Cover, UnitTypeId.Millimeters);
                        double lengthFoot = UnitUtils.ConvertToInternalUnits(footingLengthMm, UnitTypeId.Millimeters);
                        double widthFoot = UnitUtils.ConvertToInternalUnits(footingWidthMm, UnitTypeId.Millimeters);
                        double heightFoot = UnitUtils.ConvertToInternalUnits(footingHeightMm, UnitTypeId.Millimeters);

                        Transform tf = footingInstance.GetTransform();
                        XYZ uX = tf.BasisX;
                        XYZ uY = tf.BasisY;
                        XYZ uZ = tf.BasisZ;
                        XYZ footingCenter = tf.Origin;

                        double zBottom = -heightFoot + coverFoot;
                        XYZ baseCenter = footingCenter + uZ * zBottom;

                        List<Curve> curvesLong = new();
                        List<Curve> curvesShort = new();
                        XYZ normalLong, normalShort;

                        // Tham chiếu từ model (RebarData.LongHookLength, RebarData.ShortHookLength,...)
                        double hookLong = UnitUtils.ConvertToInternalUnits(RebarData.LongHookLength, UnitTypeId.Millimeters);
                        double hookShort = UnitUtils.ConvertToInternalUnits(RebarData.ShortHookLength, UnitTypeId.Millimeters);
                        double longDiaFoot = UnitUtils.ConvertToInternalUnits(RebarData.LongDiameter, UnitTypeId.Millimeters);

                        double longSpacingFoot = UnitUtils.ConvertToInternalUnits(RebarData.LongSpacing, UnitTypeId.Millimeters);
                        double shortSpacingFoot = UnitUtils.ConvertToInternalUnits(RebarData.ShortSpacing, UnitTypeId.Millimeters);

                        double totalDistLong = (RebarData.LongQuantity > 1) ? (RebarData.LongQuantity - 1) * longSpacingFoot : 0;
                        double totalDistShort = (RebarData.ShortQuantity > 1) ? (RebarData.ShortQuantity - 1) * shortSpacingFoot : 0;

                        // Công thức chung
                        // 1. Công thức tính điểm định vị hình học cho thép chữ U (Curves):
                        //    - Điểm góc uốn đáy 2 (p2/q2): baseCenter + (VectorU_Rải * startX/Y) + (VectorU * (-Cạnh_Móng / 2 + coverFoot))
                        //    - Điểm góc uốn đáy 3 (p3/q3): baseCenter + (VectorU_Rải * startX/Y) + (VectorU_Thanh * (Cạnh_Móng / 2 - coverFoot))
                        //    - Điểm đỉnh móc 1    (p1/q1): p2_or_q2 + uZ * hookLong/Short
                        //    - Điểm đỉnh móc 4    (p4/q4): p3_or_q3 + uZ * hookLong/Short
                        //
                        // 2. Quy ước phân lớp cao độ để tránh xung đột hình học:
                        //    - Thép lớp dưới (baseCenter)     : Lấy cao độ đáy zBottom = -heightFoot + coverFoot
                        //    - Thép lớp trên (baseCenterShort): Tịnh tiến lên một khoảng bằng đường kính lớp dưới = zBottom + longDiaFoot

                        if (lengthFoot >= widthFoot)
                        {
                            double startX_Long = -totalDistLong / 2;
                            XYZ p2 = baseCenter + uX * startX_Long + uY * (-lengthFoot / 2 + coverFoot);
                            XYZ p3 = baseCenter + uX * startX_Long + uY * (lengthFoot / 2 - coverFoot);
                            XYZ p1 = p2 + uZ * hookLong;
                            XYZ p4 = p3 + uZ * hookLong;
                            curvesLong.AddRange(new[] { Line.CreateBound(p1, p2), Line.CreateBound(p2, p3), Line.CreateBound(p3, p4) });

                            // Đặt theo phương Y
                            normalLong = uX;

                            double startY_Short = -totalDistShort / 2;
                            XYZ baseCenterShort = footingCenter + uZ * (zBottom + longDiaFoot);
                            XYZ q2 = baseCenterShort + uX * (-widthFoot / 2 + coverFoot) + uY * startY_Short;
                            XYZ q3 = baseCenterShort + uX * (widthFoot / 2 - coverFoot) + uY * startY_Short;
                            XYZ q1 = q2 + uZ * hookShort;
                            XYZ q4 = q3 + uZ * hookShort;
                            curvesShort.AddRange(new[] { Line.CreateBound(q1, q2), Line.CreateBound(q2, q3), Line.CreateBound(q3, q4) });

                            normalShort = uY;
                        }
                        else
                        {
                            double startY_Long = -totalDistLong / 2;
                            XYZ p2 = baseCenter + uX * (-widthFoot / 2 + coverFoot) + uY * startY_Long;
                            XYZ p3 = baseCenter + uX * (widthFoot / 2 - coverFoot) + uY * startY_Long;
                            XYZ p1 = p2 + uZ * hookLong;
                            XYZ p4 = p3 + uZ * hookLong;
                            curvesLong.AddRange(new[] { Line.CreateBound(p1, p2), Line.CreateBound(p2, p3), Line.CreateBound(p3, p4) });

                            normalLong = uY;

                            double startX_Short = -totalDistShort / 2;
                            XYZ baseCenterShort = footingCenter + uZ * (zBottom + longDiaFoot);
                            XYZ q2 = baseCenterShort + uX * startX_Short + uY * (-lengthFoot / 2 + coverFoot);
                            XYZ q3 = baseCenterShort + uX * startX_Short + uY * (lengthFoot / 2 - coverFoot);
                            XYZ q1 = q2 + uZ * hookShort;
                            XYZ q4 = q3 + uZ * hookShort;
                            curvesShort.AddRange(new[] { Line.CreateBound(q1, q2), Line.CreateBound(q2, q3), Line.CreateBound(q3, q4) });

                            normalShort = uX;
                        }

                        if (longBarType != null && curvesLong.Count > 0)
                        {
                            Rebar rebarLong = Rebar.CreateFromCurves(_doc, styleStandard, longBarType, null, null, footingElement, normalLong, curvesLong, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                            if (rebarLong != null)
                            {
                                rebarLong.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(RebarData.LongQuantity, totalDistLong, true, true, true);
                                SetRebarSolid3D(rebarLong, activeView3D);
                            }
                        }

                        if (shortBarType != null && curvesShort.Count > 0)
                        {
                            Rebar rebarShort = Rebar.CreateFromCurves(_doc, styleStandard, shortBarType, null, null, footingElement, normalShort, curvesShort, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                            if (rebarShort != null)
                            {
                                rebarShort.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(RebarData.ShortQuantity, totalDistShort, true, true, true);
                                SetRebarSolid3D(rebarShort, activeView3D);
                            }
                        }

                        //Công thức chung
                        // 1. Công thức cao độ đáy thép chờ (zStarterBottom):
                        //  zStarterBottom = zBottom + longDiaFoot + shortDiaFoot
                        //   (Nằm trên cả 2 lớp thép móng để tránh xung đột va chạm hình học)
                        //
                        // 2. Công thức chiều dài thanh thép chờ tính từ đáy (starterLenFoot):
                        //    starterLenFoot = StarterLength_nhập + |zStarterBottom|
                        //
                        // 3. Tính vị trí 4 góc chân cột (Định tâm đối xứng qua starterBaseCenter):
                        //    corner1 = starterBaseCenter + uX * (colXFoot / 2) + uY * (colYFoot / 2)
                        //    corner2 = starterBaseCenter - uX * (colXFoot / 2) + uY * (colYFoot / 2)
                        //    corner3 = starterBaseCenter - uX * (colXFoot / 2) - uY * (colYFoot / 2)
                        //    corner4 = starterBaseCenter + uX * (colXFoot / 2) - uY * (colYFoot / 2)
                        //
                        //    - uX, uY, uZ: Lần lượt là các vector chỉ phương đơn vị của hệ trục tọa độ móng.
                        //    - hookDirections: Vector chỉ phương hướng neo vào đáy móng.
                        //    - starterNormals: Vector pháp tuyến của mặt phẳng chứa thanh thép (hướng rải thép).
                        double colXFoot = UnitUtils.ConvertToInternalUnits(RebarData.ColumnWidthX, UnitTypeId.Millimeters);
                        double colYFoot = UnitUtils.ConvertToInternalUnits(RebarData.ColumnWidthY, UnitTypeId.Millimeters);
                        double starterHookFoot = UnitUtils.ConvertToInternalUnits(RebarData.StarterHookLength, UnitTypeId.Millimeters);

                        // Tính toán cao độ đặt chân thép chờ
                        double zStarterBottom = zBottom + longDiaFoot + UnitUtils.ConvertToInternalUnits(RebarData.ShortDiameter, UnitTypeId.Millimeters);

                        double starterLenFoot = UnitUtils.ConvertToInternalUnits(RebarData.StarterLength, UnitTypeId.Millimeters) + Math.Abs(zStarterBottom);

                        // Xác định tâm của mặt phẳng đặt chân thép chờ dựa theo vector chỉ phương trục đứng uZ
                        XYZ starterBaseCenter = footingCenter + uZ * zStarterBottom;

                        XYZ corner1 = starterBaseCenter + uX * (colXFoot / 2) + uY * (colYFoot / 2);
                        XYZ corner2 = starterBaseCenter + uX * (-colXFoot / 2) + uY * (colYFoot / 2);
                        XYZ corner3 = starterBaseCenter + uX * (-colXFoot / 2) + uY * (-colYFoot / 2);
                        XYZ corner4 = starterBaseCenter + uX * (colXFoot / 2) + uY * (-colYFoot / 2);

                        List<XYZ> columnCorners = new List<XYZ> { corner1, corner2, corner3, corner4 };
                        List<XYZ> hookDirections = new List<XYZ> { uX, -uX, -uX, uX };
                        List<XYZ> starterNormals = new List<XYZ> { uY, uY, uY, uY };

                        double deltaShortLenFoot = UnitUtils.ConvertToInternalUnits(180, UnitTypeId.Millimeters); // Khoảng chênh lệch chiều dài giữa thanh dài và thanh ngắn (150mm)

                        for (int i = 0; i < 4; i++)
                        {
                            XYZ cPt = columnCorners[i];
                            XYZ hDir = hookDirections[i];
                            XYZ sNormal = starterNormals[i];

                            List<Curve> sCurves = new List<Curve>();
                            XYZ pHook = cPt + hDir * starterHookFoot;

                            // Phân loại: i = 0, 2 là thanh dài; i = 1, 3 là thanh ngắn
                            double currentStarterLen = starterLenFoot;
                            if (i == 1 || i == 3)
                            {
                                currentStarterLen -= deltaShortLenFoot;
                            }

                            XYZ starterTopPoint = cPt + uZ * currentStarterLen; // starterTopPoint đỉnh chờ

                            sCurves.Add(Line.CreateBound(pHook, cPt));
                            sCurves.Add(Line.CreateBound(cPt, starterTopPoint));

                            if (starterBarType != null)
                            {
                                Rebar starterRebar = Rebar.CreateFromCurves(_doc, styleStandard, starterBarType, null, null, columnHostElement, sNormal, sCurves, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                                if (starterRebar != null) SetRebarSolid3D(starterRebar, activeView3D);
                            }
                        }

                        CurveLoop stirrupLoop = new CurveLoop();
                        stirrupLoop.Append(Line.CreateBound(corner1, corner2));
                        stirrupLoop.Append(Line.CreateBound(corner2, corner3));
                        stirrupLoop.Append(Line.CreateBound(corner3, corner4));
                        stirrupLoop.Append(Line.CreateBound(corner4, corner1));

                        if (stirrupBarType != null && stirrupLoop.Count() > 0)
                        {
                            List<Curve> finalStirrupProfile = stirrupLoop.ToList();
                            Rebar stirrupRebar = Rebar.CreateFromCurves(_doc, styleStirrup, stirrupBarType, null, null, columnHostElement, uZ, finalStirrupProfile, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);

                            if (stirrupRebar != null)
                            {
                                double stirrupSpcFoot = UnitUtils.ConvertToInternalUnits(RebarData.StirrupSpacing, UnitTypeId.Millimeters);
                                double heightInsideFooting = Math.Abs(zStarterBottom);
                                double heightOutsideFooting = stirrupSpcFoot * 2;
                                double totalStirrupDistributionLength = heightInsideFooting + heightOutsideFooting;

                                stirrupRebar.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(stirrupSpcFoot, totalStirrupDistributionLength, true, true, true);
                                SetRebarSolid3D(stirrupRebar, activeView3D);
                            }
                        }
                    }

                    trans.Commit();
                }

                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi thực thi", $"Có lỗi xảy ra: {ex.Message}");
            }
        }

        protected void SetRebarSolid3D(Rebar rebar, View3D view3D)
        {
            if (rebar == null || view3D == null) return;
            try
            {
                rebar.SetUnobscuredInView(view3D, true);
                Parameter solidParam = rebar.LookupParameter("Solid In View");
                if (solidParam != null && !solidParam.IsReadOnly)
                {
                    solidParam.Set(1);
                }
            }
            catch { }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public class FootingSelectionFilter : Autodesk.Revit.UI.Selection.ISelectionFilter
        {
            public bool AllowElement(Element elem)
            {
                return elem.Category != null && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFoundation;
            }

            public bool AllowReference(Reference reference, XYZ position)
            {
                return false;
            }
        }
    }
}