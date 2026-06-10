using AddinVeMong.Commands;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    // KHÔNG khai báo lại BarPreviewX và BarPreviewY ở đây nữa vì đã có bên ConcentricViewModel, tránh lỗi trùng lặp dữ liệu.

    public class EccentricRebarViewModel : INotifyPropertyChanged
    {
        private readonly ExternalCommandData _commandData;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        private readonly List<Element> _selectedFootings; // Lưu trữ danh sách móng đã được chọn trước đó từ Command

        #region Preview Collections
        private ObservableCollection<BarPreviewX> _previewShortBars = new ObservableCollection<BarPreviewX>();
        public ObservableCollection<BarPreviewX> PreviewShortBars
        {
            get => _previewShortBars;
            set { _previewShortBars = value; OnPropertyChanged(); }
        }

        private ObservableCollection<BarPreviewY> _previewLongBars = new ObservableCollection<BarPreviewY>();
        public ObservableCollection<BarPreviewY> PreviewLongBars
        {
            get => _previewLongBars;
            set { _previewLongBars = value; OnPropertyChanged(); }
        }
        #endregion

        #region 1. Thuộc tính Thông số chung
        private int _cover = 50;
        public int Cover
        {
            get => _cover;
            set { _cover = value; OnPropertyChanged(); }
        }
        #endregion

        #region 2. Thuộc tính Độ lệch tâm
        private int _eccentricityX = 0;
        public int EccentricityX
        {
            get => _eccentricityX;
            set { _eccentricityX = value; OnPropertyChanged(); }
        }

        private int _eccentricityY = 0;
        public int EccentricityY
        {
            get => _eccentricityY;
            set { _eccentricityY = value; OnPropertyChanged(); }
        }
        #endregion

        #region 3. Thuộc tính Thép cạnh dài
        private int _longDiameter = 12;
        public int LongDiameter
        {
            get => _longDiameter;
            set { _longDiameter = value; OnPropertyChanged(); }
        }

        private int _longSpacing = 150;
        public int LongSpacing
        {
            get => _longSpacing;
            set { _longSpacing = value; OnPropertyChanged(); }
        }

        private int _longQuantity = 10;
        public int LongQuantity
        {
            get => _longQuantity;
            set
            {
                _longQuantity = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        private int _longHookLength = 150;
        public int LongHookLength
        {
            get => _longHookLength;
            set { _longHookLength = value; OnPropertyChanged(); }
        }
        #endregion

        #region 4. Thuộc tính Thép cạnh ngắn
        private int _shortDiameter = 12;
        public int ShortDiameter
        {
            get => _shortDiameter;
            set { _shortDiameter = value; OnPropertyChanged(); }
        }

        private int _shortSpacing = 150;
        public int ShortSpacing
        {
            get => _shortSpacing;
            set { _shortSpacing = value; OnPropertyChanged(); }
        }

        private int _shortQuantity = 8;
        public int ShortQuantity
        {
            get => _shortQuantity;
            set
            {
                _shortQuantity = value;
                OnPropertyChanged();
                UpdatePreview();
            }
        }

        private int _shortHookLength = 150;
        public int ShortHookLength
        {
            get => _shortHookLength;
            set { _shortHookLength = value; OnPropertyChanged(); }
        }
        #endregion

        #region 5. Thuộc tính Thép cổ cột và Thép đai
        private int _columnWidthX = 300;
        public int ColumnWidthX
        {
            get => _columnWidthX;
            set { _columnWidthX = value; OnPropertyChanged(); }
        }

        private int _columnWidthY = 300;
        public int ColumnWidthY
        {
            get => _columnWidthY;
            set { _columnWidthY = value; OnPropertyChanged(); }
        }

        private int _starterDiameter = 18;
        public int StarterDiameter
        {
            get => _starterDiameter;
            set { _starterDiameter = value; OnPropertyChanged(); }
        }

        private int _starterHookLength = 250;
        public int StarterHookLength
        {
            get => _starterHookLength;
            set { _starterHookLength = value; OnPropertyChanged(); }
        }

        private int _starterLength = 600;
        public int StarterLength
        {
            get => _starterLength;
            set { _starterLength = value; OnPropertyChanged(); }
        }

        private int _stirrupDiameter = 6;
        public int StirrupDiameter
        {
            get => _stirrupDiameter;
            set { _stirrupDiameter = value; OnPropertyChanged(); }
        }

        private int _stirrupSpacing = 150;
        public int StirrupSpacing
        {
            get => _stirrupSpacing;
            set { _stirrupSpacing = value; OnPropertyChanged(); }
        }
        #endregion

        public ICommand DrawRebarCommand { get; }

        public EccentricRebarViewModel(ExternalCommandData commandData, List<Element> selectedFootings)
        {
            _commandData = commandData;
            _doc = commandData.Application.ActiveUIDocument.Document;
            _uiDoc = commandData.Application.ActiveUIDocument;
            _selectedFootings = selectedFootings ?? new List<Element>();

            DrawRebarCommand = new RelayCommand(ExecuteDrawRebar);

            UpdatePreview();
        }

        private void UpdatePreview()
        {
            // Cập nhật Thép cạnh ngắn (phương Y) cho giao diện UI Preview
            PreviewShortBars.Clear();
            int shortQty = ShortQuantity;
            if (shortQty > 1)
            {
                double startX = 5;
                double endX = 265;
                double step = (endX - startX) / (shortQty - 1);
                for (int i = 0; i < shortQty; i++)
                {
                    PreviewShortBars.Add(new BarPreviewX { XPosition = startX + (i * step) });
                }
            }

            // Cập nhật Thép cạnh dài (phương X) cho giao diện UI Preview
            PreviewLongBars.Clear();
            int longQty = LongQuantity;
            if (longQty > 1)
            {
                double startY = 5;
                double endY = 175;
                double step = (endY - startY) / (longQty - 1);
                for (int i = 0; i < longQty; i++)
                {
                    PreviewLongBars.Add(new BarPreviewY { YPosition = startY + (i * step) });
                }
            }
        }

        private void ExecuteDrawRebar(object parameter)
        {
            try
            {
                if (_selectedFootings == null || _selectedFootings.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Không tìm thấy dữ liệu móng được chọn.");
                    return;
                }

                View3D activeView3D = _doc.ActiveView as View3D;

                using (Transaction trans = new Transaction(_doc, "Vẽ Thép Móng Lệch Tâm Hàng Loạt"))
                {
                    trans.Start();

                    // Tìm các kiểu đường kính RebarBarType trong dự án
                    var barTypes = new FilteredElementCollector(_doc)
                        .OfClass(typeof(RebarBarType))
                        .Cast<RebarBarType>()
                        .ToList();

                    if (barTypes.Count == 0)
                    {
                        TaskDialog.Show("Thiếu Dữ Liệu", "Dự án chưa được tải Family Thép.");
                        trans.RollBack();
                        return;
                    }

                    RebarBarType longBarType = barTypes.FirstOrDefault(t => t.Name.Contains(this.LongDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - this.LongDiameter) < 0.1);
                    RebarBarType shortBarType = barTypes.FirstOrDefault(t => t.Name.Contains(this.ShortDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - this.ShortDiameter) < 0.1);
                    RebarBarType starterBarType = barTypes.FirstOrDefault(t => t.Name.Contains(this.StarterDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - this.StarterDiameter) < 0.1);
                    RebarBarType stirrupBarType = barTypes.FirstOrDefault(t => t.Name.Contains(this.StirrupDiameter.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - this.StirrupDiameter) < 0.1);

                    if (longBarType == null) longBarType = barTypes.FirstOrDefault();
                    if (shortBarType == null) shortBarType = barTypes.FirstOrDefault();
                    if (starterBarType == null) starterBarType = barTypes.FirstOrDefault();
                    if (stirrupBarType == null) stirrupBarType = barTypes.FirstOrDefault();

                    RebarStyle styleStandard = RebarStyle.Standard;
                    RebarStyle styleStirrup = RebarStyle.StirrupTie;

                    // Lặp qua từng móng đơn được chọn
                    foreach (Element footingElement in _selectedFootings)
                    {
                        FamilyInstance footingInstance = footingElement as FamilyInstance;
                        if (footingInstance == null) continue;

                        // Tìm cột tương ứng giao với móng hiện tại (Giống hệt Concentric)
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

                        // Quy đổi đơn vị sang Internal (Feet)
                        double coverFoot = UnitUtils.ConvertToInternalUnits(this.Cover, UnitTypeId.Millimeters);
                        double lengthFoot = UnitUtils.ConvertToInternalUnits(footingLengthMm, UnitTypeId.Millimeters);
                        double widthFoot = UnitUtils.ConvertToInternalUnits(footingWidthMm, UnitTypeId.Millimeters);
                        double heightFoot = UnitUtils.ConvertToInternalUnits(footingHeightMm, UnitTypeId.Millimeters);

                        // Lấy Hệ tọa độ cục bộ của móng
                        Transform tf = footingInstance.GetTransform();
                        XYZ uX = tf.BasisX; // Hướng X cấu kiện
                        XYZ uY = tf.BasisY; // Hướng Y cấu kiện
                        XYZ uZ = tf.BasisZ; // Hướng Z cấu kiện
                        XYZ footingCenter = tf.Origin;

                        // Điểm cao độ đáy móng (đã trừ lớp bảo vệ)
                        double zBottom = -heightFoot + coverFoot;
                        XYZ baseCenter = footingCenter + uZ * zBottom;

                        List<Curve> curvesLong = new List<Curve>();
                        List<Curve> curvesShort = new List<Curve>();
                        XYZ normalLong, normalShort;

                        // Chuyển thông số móc thép sang Feet
                        double hookLong = UnitUtils.ConvertToInternalUnits(this.LongHookLength, UnitTypeId.Millimeters);
                        double hookShort = UnitUtils.ConvertToInternalUnits(this.ShortHookLength, UnitTypeId.Millimeters);
                        double longDiaFoot = UnitUtils.ConvertToInternalUnits(this.LongDiameter, UnitTypeId.Millimeters);

                        double longSpacingFoot = UnitUtils.ConvertToInternalUnits(this.LongSpacing, UnitTypeId.Millimeters);
                        double shortSpacingFoot = UnitUtils.ConvertToInternalUnits(this.ShortSpacing, UnitTypeId.Millimeters);

                        double totalDistLong = (this.LongQuantity > 1) ? longSpacingFoot * (this.LongQuantity - 1) : 0;
                        double totalDistShort = (this.ShortQuantity > 1) ? shortSpacingFoot * (this.ShortQuantity - 1) : 0;

                        // --- ÁP DỤNG THUẬT TOÁN ĐẶT THÉP MÓNG PHƯƠNG X VÀ Y Y HỆT MÃ CONCENTRIC ---
                        if (lengthFoot >= widthFoot)
                        {
                            double startX_Long = -totalDistLong / 2;
                            XYZ p2 = baseCenter + uX * startX_Long + uY * (-lengthFoot / 2 + coverFoot);
                            XYZ p3 = baseCenter + uX * startX_Long + uY * (lengthFoot / 2 - coverFoot);
                            XYZ p1 = p2 + uZ * hookLong;
                            XYZ p4 = p3 + uZ * hookLong;
                            curvesLong.AddRange(new[] { Line.CreateBound(p1, p2), Line.CreateBound(p2, p3), Line.CreateBound(p3, p4) });
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

                        // SỬA LỖI: Gọi đúng overload 12 đối số cho Rebar thép móng phương dọc (X)
                        if (longBarType != null && curvesLong.Count > 0)
                        {
                            Rebar rebarLong = Rebar.CreateFromCurves(_doc, styleStandard, longBarType, null, null, footingElement, normalLong, curvesLong, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                            if (rebarLong != null)
                            {
                                rebarLong.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(this.LongQuantity, totalDistLong, true, true, true);
                                SetRebarSolid3D(rebarLong, activeView3D);
                            }
                        }

                        // SỬA LỖI: Gọi đúng overload 12 đối số cho Rebar thép móng phương ngang (Y)
                        if (shortBarType != null && curvesShort.Count > 0)
                        {
                            Rebar rebarShort = Rebar.CreateFromCurves(_doc, styleStandard, shortBarType, null, null, footingElement, normalShort, curvesShort, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                            if (rebarShort != null)
                            {
                                rebarShort.GetShapeDrivenAccessor().SetLayoutAsFixedNumber(this.ShortQuantity, totalDistShort, true, true, true);
                                SetRebarSolid3D(rebarShort, activeView3D);
                            }
                        }

                        // --- 3. DỰNG THÉP CHỜ CỔ CỘT VÀ ĐAI THEO ĐỘ LỆCH TÂM BIẾN LƯU GIAO DIỆN ECCENTRIC ---
                        double colXFoot = UnitUtils.ConvertToInternalUnits(this.ColumnWidthX, UnitTypeId.Millimeters);
                        double colYFoot = UnitUtils.ConvertToInternalUnits(this.ColumnWidthY, UnitTypeId.Millimeters);
                        double eccXFoot = UnitUtils.ConvertToInternalUnits(this.EccentricityX, UnitTypeId.Millimeters);
                        double eccYFoot = UnitUtils.ConvertToInternalUnits(this.EccentricityY, UnitTypeId.Millimeters);

                        double starterHookFoot = UnitUtils.ConvertToInternalUnits(this.StarterHookLength, UnitTypeId.Millimeters);
                        double starterLenFoot = UnitUtils.ConvertToInternalUnits(this.StarterLength, UnitTypeId.Millimeters);

                        double zStarterBottom = zBottom + longDiaFoot + UnitUtils.ConvertToInternalUnits(this.ShortDiameter, UnitTypeId.Millimeters);

                        // Áp dụng độ lệch tâm eccXFoot, eccYFoot để tìm tâm thực tế của vị trí cổ cột lệch
                        XYZ columnCenter = footingCenter + (uX * eccXFoot) + (uY * eccYFoot);
                        XYZ starterBaseCenter = columnCenter + uZ * zStarterBottom;

                        XYZ corner1 = starterBaseCenter + uX * (colXFoot / 2) + uY * (colYFoot / 2);
                        XYZ corner2 = starterBaseCenter + uX * (-colXFoot / 2) + uY * (colYFoot / 2);
                        XYZ corner3 = starterBaseCenter + uX * (-colXFoot / 2) + uY * (-colYFoot / 2);
                        XYZ corner4 = starterBaseCenter + uX * (colXFoot / 2) + uY * (-colYFoot / 2);

                        List<XYZ> columnCorners = new List<XYZ> { corner1, corner2, corner3, corner4 };
                        List<XYZ> hookDirections = new List<XYZ> { uX, -uX, -uX, uX };
                        List<XYZ> starterNormals = new List<XYZ> { uY, uY, uY, uY };

                        // Tạo 4 thanh thép dọc cổ cột lệch tâm
                        for (int i = 0; i < 4; i++)
                        {
                            XYZ cPt = columnCorners[i];
                            XYZ hDir = hookDirections[i];
                            XYZ sNormal = starterNormals[i];

                            List<Curve> sCurves = new List<Curve>();
                            XYZ pBẻChân = cPt + hDir * starterHookFoot;
                            XYZ pĐỉnhChờ = cPt + uZ * (starterLenFoot + Math.Abs(zStarterBottom));

                            sCurves.Add(Line.CreateBound(pBẻChân, cPt));
                            sCurves.Add(Line.CreateBound(cPt, pĐỉnhChờ));

                            // SỬA LỖI: Sử dụng overload 12 đối số cho thép chờ cổ cột
                            if (starterBarType != null)
                            {
                                Rebar starterRebar = Rebar.CreateFromCurves(_doc, styleStandard, starterBarType, null, null, columnHostElement, sNormal, sCurves, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                                if (starterRebar != null) SetRebarSolid3D(starterRebar, activeView3D);
                            }
                        }

                        // --- 4. TẠO THÉP ĐAI CỘT LỆCH TÂM ---
                        CurveLoop stirrupLoop = new CurveLoop();
                        stirrupLoop.Append(Line.CreateBound(corner1, corner2));
                        stirrupLoop.Append(Line.CreateBound(corner2, corner3));
                        stirrupLoop.Append(Line.CreateBound(corner3, corner4));
                        stirrupLoop.Append(Line.CreateBound(corner4, corner1));

                        // SỬA LỖI: Sử dụng overload 12 đối số cho thép đai cột
                        if (stirrupBarType != null && stirrupLoop.Count() > 0)
                        {
                            List<Curve> finalStirrupProfile = stirrupLoop.ToList();
                            Rebar stirrupRebar = Rebar.CreateFromCurves(_doc, styleStirrup, stirrupBarType, null, null, columnHostElement, uZ, finalStirrupProfile, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);

                            if (stirrupRebar != null)
                            {
                                double stirrupSpcFoot = UnitUtils.ConvertToInternalUnits(this.StirrupSpacing, UnitTypeId.Millimeters);
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

                // Đóng Window UI sau khi thực thi vẽ thành công
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

        private void SetRebarSolid3D(Rebar rebar, View3D view3D)
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}