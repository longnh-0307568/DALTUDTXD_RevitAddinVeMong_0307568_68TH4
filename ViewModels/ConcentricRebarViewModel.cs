using AddinVeMong.Commands;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Structure;
using Autodesk.Revit.UI;
using System;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
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

        #region 1. Thuộc tính Thông số chung
        private int _cover = 50;
        public int Cover
        {
            get => _cover;
            set { _cover = value; OnPropertyChanged(); }
        }
        #endregion

        #region 2. Thuộc tính Thép cạnh dài
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

        private int _longHookLength = 150;
        public int LongHookLength
        {
            get => _longHookLength;
            set { _longHookLength = value; OnPropertyChanged(); }
        }
        #endregion

        #region 3. Thuộc tính Thép cạnh ngắn
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

        private int _shortHookLength = 150;
        public int ShortHookLength
        {
            get => _shortHookLength;
            set { _shortHookLength = value; OnPropertyChanged(); }
        }
        #endregion

        #region 4. Thuộc tính Thép cổ cột & Đai
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
        #endregion

        #region 5. Lệnh thực thi (Command)
        public ICommand DrawRebarCommand { get; }
        #endregion

        public ConcentricRebarViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _uiDoc = commandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;

            DrawRebarCommand = new RelayCommand(ExecuteDrawRebar, CanExecuteDrawRebar);
        }

        private bool CanExecuteDrawRebar(object parameter)
        {
            return true;
        }

        private void ExecuteDrawRebar(object parameter)
        {
            try
            {
                var selectedIds = _uiDoc.Selection.GetElementIds();
                if (selectedIds.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Vui lòng chọn một cấu kiện Móng đơn trước khi bấm Tạo Thép!");
                    return;
                }

                Element footingElement = _doc.GetElement(selectedIds.First());
                if (footingElement.Category == null || footingElement.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StructuralFoundation)
                {
                    TaskDialog.Show("Lỗi dữ liệu", "Đối tượng Bạn chọn không phải là Móng đơn. Vui lòng chọn lại!");
                    return;
                }

                FamilyInstance footingInstance = footingElement as FamilyInstance;
                if (footingInstance == null) return;

                ElementType footingType = _doc.GetElement(footingElement.GetTypeId()) as ElementType;
                if (footingType == null) return;

                Parameter pLength = footingType.LookupParameter("Length");
                Parameter pWidth = footingType.LookupParameter("Width");
                Parameter pHeight = footingType.LookupParameter("Foundation Thickness") ?? footingType.LookupParameter("Thickness");

                if (pLength == null || pWidth == null || pHeight == null)
                {
                    TaskDialog.Show("Lỗi Family", "Không tìm thấy các thông số thuộc tính kích thước trong Family móng này.");
                    return;
                }

                double footingLengthMm = UnitUtils.ConvertFromInternalUnits(pLength.AsDouble(), UnitTypeId.Millimeters);
                double footingWidthMm = UnitUtils.ConvertFromInternalUnits(pWidth.AsDouble(), UnitTypeId.Millimeters);
                double footingHeightMm = UnitUtils.ConvertFromInternalUnits(pHeight.AsDouble(), UnitTypeId.Millimeters);

                int coverValue = this.Cover;
                int longDia = this.LongDiameter;
                int longSpc = this.LongSpacing;
                int shortDia = this.ShortDiameter;
                int shortSpc = this.ShortSpacing;

                // Trích xuất hệ toạ độ xoay cục bộ của móng
                Transform tf = footingInstance.GetTransform();
                XYZ uX = tf.BasisX; // Hướng trục X cục bộ (gắn với tham số Width trong family này)
                XYZ uY = tf.BasisY; // Hướng trục Y cục bộ (gắn với tham số Length trong family này)
                XYZ uZ = tf.BasisZ;
                XYZ footingCenter = tf.Origin;

                using (Transaction trans = new Transaction(_doc, "Tạo Lưới Thép Đáy Móng"))
                {
                    trans.Start();

                    var barTypes = new FilteredElementCollector(_doc)
                        .OfClass(typeof(RebarBarType))
                        .Cast<RebarBarType>()
                        .ToList();

                    if (barTypes.Count == 0)
                    {
                        TaskDialog.Show("Thiếu Dữ Liệu", "Dự án chưa load Family Thép. Vui lòng load Family thép trước!");
                        trans.RollBack();
                        return;
                    }

                    RebarBarType longBarType = barTypes.FirstOrDefault(t => t.Name.Contains(longDia.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - longDia) < 0.1);
                    RebarBarType shortBarType = barTypes.FirstOrDefault(t => t.Name.Contains(shortDia.ToString()) || Math.Abs(UnitUtils.ConvertFromInternalUnits(t.get_Parameter(BuiltInParameter.REBAR_BAR_DIAMETER).AsDouble(), UnitTypeId.Millimeters) - shortDia) < 0.1);

                    if (longBarType == null) longBarType = barTypes.FirstOrDefault();
                    if (shortBarType == null) shortBarType = barTypes.FirstOrDefault();

                    RebarStyle style = RebarStyle.Standard;

                    double coverFoot = UnitUtils.ConvertToInternalUnits(coverValue, UnitTypeId.Millimeters);
                    double lengthFoot = UnitUtils.ConvertToInternalUnits(footingLengthMm, UnitTypeId.Millimeters); // Chiều dọc (uY)
                    double widthFoot = UnitUtils.ConvertToInternalUnits(footingWidthMm, UnitTypeId.Millimeters);   // Chiều ngang (uX)
                    double heightFoot = UnitUtils.ConvertToInternalUnits(footingHeightMm, UnitTypeId.Millimeters);

                    double zBottom = -heightFoot + coverFoot;
                    XYZ baseCenter = footingCenter + uZ * zBottom;

                    List<Curve> curvesLong = new List<Curve>();
                    List<Curve> curvesShort = new List<Curve>();
                    XYZ normalLong;
                    XYZ normalShort;
                    double distWidthLong;
                    double distWidthShort;

                    double hookLong = UnitUtils.ConvertToInternalUnits(this.LongHookLength, UnitTypeId.Millimeters);
                    double hookShort = UnitUtils.ConvertToInternalUnits(this.ShortHookLength, UnitTypeId.Millimeters);
                    double longDiaFoot = UnitUtils.ConvertToInternalUnits(longDia, UnitTypeId.Millimeters);

                    // ⭐ THUẬT TOÁN ĐÃ ĐƯỢC CHUẨN HOÁ THEO QUY CÁCH HIỂN THỊ THỰC TẾ:
                    if (lengthFoot >= widthFoot)
                    {
                        // TRƯỜNG HỢP MÓNG DỌC (Giống hình của Bạn): Cạnh dọc uY dài hơn cạnh ngang uX

                        // 1. Thép dài phải chạy DỌC theo trục uY, mặt phẳng đứng có pháp tuyến là uX
                        XYZ p2 = baseCenter + uX * (-widthFoot / 2 + coverFoot) + uY * (-lengthFoot / 2 + coverFoot);
                        XYZ p3 = baseCenter + uX * (-widthFoot / 2 + coverFoot) + uY * (lengthFoot / 2 - coverFoot);
                        XYZ p1 = p2 + uZ * hookLong;
                        XYZ p4 = p3 + uZ * hookLong;
                        curvesLong.AddRange(new[] { Line.CreateBound(p1, p2), Line.CreateBound(p2, p3), Line.CreateBound(p3, p4) });

                        normalLong = uX; // Rải tịnh tiến theo phương ngang uX
                        distWidthLong = widthFoot - (2 * coverFoot);

                        // 2. Thép ngắn chạy NGANG theo trục uX, nằm đè lên thép dài, mặt phẳng đứng có pháp tuyến là uY
                        XYZ baseCenterShort = footingCenter + uZ * (zBottom + longDiaFoot);
                        XYZ q2 = baseCenterShort + uX * (-widthFoot / 2 + coverFoot) + uY * (-lengthFoot / 2 + coverFoot);
                        XYZ q3 = baseCenterShort + uX * (widthFoot / 2 - coverFoot) + uY * (-lengthFoot / 2 + coverFoot);
                        XYZ q1 = q2 + uZ * hookShort;
                        XYZ q4 = q3 + uZ * hookShort;
                        curvesShort.AddRange(new[] { Line.CreateBound(q1, q2), Line.CreateBound(q2, q3), Line.CreateBound(q3, q4) });

                        normalShort = uY; // Rải tịnh tiến theo phương dọc uY
                        distWidthShort = lengthFoot - (2 * coverFoot);
                    }
                    else
                    {
                        // TRƯỜNG HỢP MÓNG NGANG: Cạnh ngang uX dài hơn cạnh dọc uY

                        // 1. Thép dài chạy NGANG theo trục uX, mặt phẳng đứng có pháp tuyến là uY
                        XYZ p2 = baseCenter + uX * (-widthFoot / 2 + coverFoot) + uY * (-lengthFoot / 2 + coverFoot);
                        XYZ p3 = baseCenter + uX * (widthFoot / 2 - coverFoot) + uY * (-lengthFoot / 2 + coverFoot);
                        XYZ p1 = p2 + uZ * hookLong;
                        XYZ p4 = p3 + uZ * hookLong;
                        curvesLong.AddRange(new[] { Line.CreateBound(p1, p2), Line.CreateBound(p2, p3), Line.CreateBound(p3, p4) });

                        normalLong = uY; // Rải tịnh tiến theo phương dọc uY
                        distWidthLong = lengthFoot - (2 * coverFoot);

                        // 2. Thép ngắn chạy DỌC theo trục uY, nằm đè lên thép dài, mặt phẳng đứng có pháp tuyến là uX
                        XYZ baseCenterShort = footingCenter + uZ * (zBottom + longDiaFoot);
                        XYZ q2 = baseCenterShort + uX * (-widthFoot / 2 + coverFoot) + uY * (-lengthFoot / 2 + coverFoot);
                        XYZ q3 = baseCenterShort + uX * (-widthFoot / 2 + coverFoot) + uY * (lengthFoot / 2 - coverFoot);
                        XYZ q1 = q2 + uZ * hookShort;
                        XYZ q4 = q3 + uZ * hookShort;
                        curvesShort.AddRange(new[] { Line.CreateBound(q1, q2), Line.CreateBound(q2, q3), Line.CreateBound(q3, q4) });

                        normalShort = uX; // Rải tịnh tiến theo phương ngang uX
                        distWidthShort = widthFoot - (2 * coverFoot);
                    }

                    // ============================================================
                    // BƯỚC 5: TIẾN HÀNH SINH THÉP VÀ PHÂN PHỐI RẢI
                    // ============================================================
                    if (longBarType != null && curvesLong.Count > 0)
                    {
                        Rebar rebarLong = Rebar.CreateFromCurves(_doc, style, longBarType, null, null, footingElement, normalLong, curvesLong, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                        if (rebarLong != null)
                        {
                            double spacingLongFoot = UnitUtils.ConvertToInternalUnits(longSpc, UnitTypeId.Millimeters);
                            rebarLong.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(spacingLongFoot, distWidthLong, true, true, true);
                        }
                    }

                    if (shortBarType != null && curvesShort.Count > 0)
                    {
                        Rebar rebarShort = Rebar.CreateFromCurves(_doc, style, shortBarType, null, null, footingElement, normalShort, curvesShort, RebarHookOrientation.Right, RebarHookOrientation.Right, true, true);
                        if (rebarShort != null)
                        {
                            double spacingShortFoot = UnitUtils.ConvertToInternalUnits(shortSpc, UnitTypeId.Millimeters);
                            rebarShort.GetShapeDrivenAccessor().SetLayoutAsMaximumSpacing(spacingShortFoot, distWidthShort, true, true, true);
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

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}