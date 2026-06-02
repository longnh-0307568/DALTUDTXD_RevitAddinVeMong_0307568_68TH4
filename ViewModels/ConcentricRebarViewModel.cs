using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AddinVeMong.Commands;

namespace AddinVeMong.ViewModels
{
    public class ConcentricRebarViewModel : INotifyPropertyChanged
    {
        // Lưu trữ các đối tượng context của Revit API
        private readonly ExternalCommandData _commandData;
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;

        #region 1. Thuộc tính Thông số chung
        private int _cover = 50; // Mặc định 50mm
        public int Cover
        {
            get => _cover;
            set { _cover = value; OnPropertyChanged(); }
        }
        #endregion

        #region 2. Thuộc tính Thép cạnh dài (Phương X)
        private int _longDiameter = 12; // Mặc định Phi 12
        public int LongDiameter
        {
            get => _longDiameter;
            set { _longDiameter = value; OnPropertyChanged(); }
        }

        private int _longSpacing = 150; // Mặc định a150
        public int LongSpacing
        {
            get => _longSpacing;
            set { _longSpacing = value; OnPropertyChanged(); }
        }

        private int _longHookLength = 150; // Mặc định bẻ móc 150mm
        public int LongHookLength
        {
            get => _longHookLength;
            set { _longHookLength = value; OnPropertyChanged(); }
        }
        #endregion

        #region 3. Thuộc tính Thép cạnh ngắn (Phương Y)
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
        private int _starterDiameter = 18; // Mặc định Phi 18
        public int StarterDiameter
        {
            get => _starterDiameter;
            set { _starterDiameter = value; OnPropertyChanged(); }
        }

        private int _starterHookLength = 250; // Bẻ chân vịt 250mm
        public int StarterHookLength
        {
            get => _starterHookLength;
            set { _starterHookLength = value; OnPropertyChanged(); }
        }

        private int _starterLength = 600; // Chiều dài chờ thò lên 600mm
        public int StarterLength
        {
            get => _starterLength;
            set { _starterLength = value; OnPropertyChanged(); }
        }

        private int _stirrupDiameter = 6; // Thép đai Phi 6
        public int StirrupDiameter
        {
            get => _stirrupDiameter;
            set { _stirrupDiameter = value; OnPropertyChanged(); }
        }

        private int _stirrupSpacing = 150; // Khoảng cách đai a150
        public int StirrupSpacing
        {
            get => _stirrupSpacing;
            set { _stirrupSpacing = value; OnPropertyChanged(); }
        }

        private int _columnWidthX = 300; // Mặc định cột rộng 300mm phương X
        public int ColumnWidthX
        {
            get => _columnWidthX;
            set { _columnWidthX = value; OnPropertyChanged(); }
        }

        private int _columnWidthY = 300; // Mặc định cột rộng 300mm phương Y
        public int ColumnWidthY
        {
            get => _columnWidthY;
            set { _columnWidthY = value; OnPropertyChanged(); }
        }
        #endregion

        #region 5. Lệnh thực thi (Command)
        public ICommand DrawRebarCommand { get; }
        #endregion

        // Hàm khởi tạo nhận vào ngữ cảnh của Revit
        public ConcentricRebarViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            _uiDoc = commandData.Application.ActiveUIDocument;
            _doc = _uiDoc.Document;

            // Khởi tạo lệnh vẽ thép liên kết với nút bấm trên UI
            DrawRebarCommand = new RelayCommand(ExecuteDrawRebar, CanExecuteDrawRebar);
        }

        // Điều kiện kích hoạt nút "Tạo Thép" (Có thể bổ sung logic check lỗi nhập liệu ở đây)
        private bool CanExecuteDrawRebar(object parameter)
        {
            return true;
        }

        // Nơi xử lý thuật toán vẽ thép cốt lõi của Revit API
        private void ExecuteDrawRebar(object parameter)
        {
            try
            {
                // 1. LẤY ĐỐI TƯỢNG ĐANG CHỌN TRÊN MÀN HÌNH REVIT
                var selectedIds = _uiDoc.Selection.GetElementIds();

                if (selectedIds.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Vui lòng chọn một cấu kiện Móng đơn trước khi bấm Tạo Thép!");
                    return;
                }

                // Lấy ra phần tử đầu tiên được chọn từ danh sách người dùng quét chuột
                Element footingElement = _doc.GetElement(selectedIds.First());

                // Kiểm tra xem cấu kiện được chọn có phải thuộc danh mục Móng (Structural Foundations) hay không
                if (footingElement.Category == null || footingElement.Category.Id.IntegerValue != (int)BuiltInCategory.OST_StructuralFoundation)
                {
                    TaskDialog.Show("Lỗi dữ liệu", "Đối tượng Bạn chọn không phải là Móng đơn (Structural Foundation). Vui lòng chọn lại!");
                    return;
                }

                // 2. BÓC TÁCH KÍCH THƯỚC HÌNH HỌC CỦA MÓNG
                // Lấy dữ liệu Type (Edit Type) của cấu kiện móng
                ElementType footingType = _doc.GetElement(footingElement.GetTypeId()) as ElementType;

                if (footingType == null)
                {
                    TaskDialog.Show("Lỗi", "Không thể truy cập dữ liệu Type của móng.");
                    return;
                }

                // Tìm các Parameter quy định kích thước hình học móng
                Parameter pLength = footingType.LookupParameter("Length"); // Chiều dài móng (Phương X)
                Parameter pWidth = footingType.LookupParameter("Width");   // Chiều rộng móng (Phương Y)
                Parameter pHeight = footingType.LookupParameter("Foundation Thickness") ?? footingType.LookupParameter("Thickness"); // Chiều cao móng

                if (pLength == null || pWidth == null || pHeight == null)
                {
                    TaskDialog.Show("Lỗi Family", "Không tìm thấy các thông số thuộc tính kích thước (Length, Width, Thickness) trong Family móng này. Vui lòng kiểm tra lại tên Parameter của Family!");
                    return;
                }

                // Đổi đơn vị kích thước móng từ đơn vị Internal của Revit (Foot) sang Millimeter (mm) để tính toán
                double footingLengthMm = UnitUtils.ConvertFromInternalUnits(pLength.AsDouble(), UnitTypeId.Millimeters);
                double footingWidthMm = UnitUtils.ConvertFromInternalUnits(pWidth.AsDouble(), UnitTypeId.Millimeters);
                double footingHeightMm = UnitUtils.ConvertFromInternalUnits(pHeight.AsDouble(), UnitTypeId.Millimeters);

                // 3. LẤY CÁC THÔNG SỐ NGƯỜI DÙNG ĐÃ NHẬP TỪ FORM
                int coverValue = this.Cover;
                int colX = this.ColumnWidthX;
                int colY = this.ColumnWidthY;
                int longDia = this.LongDiameter;
                int longSpc = this.LongSpacing;
                int shortDia = this.ShortDiameter;
                int shortSpc = this.ShortSpacing;

                // 4. HIỂN THỊ THÔNG BÁO KIỂM TRA TOÀN BỘ LOGIC HÌNH HỌC TRƯỚC KHI VẼ
                string infoLog = $"--- THÔNG TIN HÌNH HỌC MÓNG ---\n" +
                                 $"+ Tên cấu kiện: {footingElement.Name}\n" +
                                 $"+ Chiều dài móng L (X): {footingLengthMm} mm\n" +
                                 $"+ Chiều rộng móng B (Y): {footingWidthMm} mm\n" +
                                 $"+ Chiều cao móng H (Z): {footingHeightMm} mm\n\n" +
                                 $"--- THÔNG TIN CỔ CỘT NHẬP VÀO ---\n" +
                                 $"+ Kích thước cổ cột X: {colX} mm\n" +
                                 $"+ Kích thước cổ cột Y: {colY} mm\n\n" +
                                 $"--- THÔNG SỐ THÉP ĐÃ CHỌN ---\n" +
                                 $"+ Lớp bảo vệ móng: {coverValue} mm\n" +
                                 $"+ Thép dọc (X): Phi {longDia} - Khoảng cách: a{longSpc}\n" +
                                 $"+ Thép ngang (Y): Phi {shortDia} - Khoảng cách: a{shortSpc}\n\n" +
                                 $"Hệ thống đã bóc tách dữ liệu thành công! Sẵn sàng mở Transaction để tạo thép.";

                // Tạm thời comment out bảng thông báo rà soát như Bạn mong muốn
                // TaskDialog.Show("Rà soát thuật toán hình học", infoLog);

                // ============================================================
                // BƯỚC 2: KHỞI ĐỘNG GIAO DỊCH (OPEN TRANSACTION)
                // ============================================================
                using (Transaction trans = new Transaction(_doc, "Tạo Thép Đáy Móng"))
                {
                    trans.Start();

                    // Đổi đơn vị Lớp bảo vệ sang đơn vị nội bộ của Revit (Foot) để tính toán tọa độ
                    double coverFoot = UnitUtils.ConvertToInternalUnits(coverValue, UnitTypeId.Millimeters);

                    // Cao độ đáy lưới thép cách mặt đáy móng một khoảng bằng lớp bảo vệ bê tông
                    double zBottom = coverFoot;

                    // ============================================================
                    // BƯỚC 3: TÍNH TOÁN ĐƯỜNG CURVE CHO THÉP CẠNH DÀI (PHƯƠNG X)
                    // ============================================================
                    // Đổi các thông số kích thước hình học sang dạng Foot
                    double lengthFoot = UnitUtils.ConvertToInternalUnits(footingLengthMm, UnitTypeId.Millimeters);
                    double widthFoot = UnitUtils.ConvertToInternalUnits(footingWidthMm, UnitTypeId.Millimeters);
                    double hookXFoot = UnitUtils.ConvertToInternalUnits(this.LongHookLength, UnitTypeId.Millimeters);

                    // Xác định tọa độ giới hạn của thanh thép theo phương X (Đã trừ lớp bảo vệ 2 đầu móng)
                    double xStart = -lengthFoot / 2 + coverFoot;
                    double xEnd = lengthFoot / 2 - coverFoot;

                    // Thanh thép đầu tiên nằm sát mép bảo vệ phương Y của móng đơn đúng tâm
                    double yPositionX = -widthFoot / 2 + coverFoot;

                    // Khởi tạo 4 điểm nút tọa độ để tạo dựng hình dáng chữ U ngửa phương X
                    XYZ p1 = new XYZ(xStart, yPositionX, zBottom + hookXFoot); // Đỉnh móc trái
                    XYZ p2 = new XYZ(xStart, yPositionX, zBottom);             // Góc vuông dưới trái
                    XYZ p3 = new XYZ(xEnd, yPositionX, zBottom);               // Góc vuông dưới phải
                    XYZ p4 = new XYZ(xEnd, yPositionX, zBottom + hookXFoot);   // Đỉnh móc phải

                    // Gom 3 đoạn thẳng lại thành bộ khung xương hình học thanh thép
                    List<Curve> curvesX = new List<Curve>
                    {
                        Line.CreateBound(p1, p2), // Đoạn móc neo trái
                        Line.CreateBound(p2, p3), // Đoạn thân nằm ngang
                        Line.CreateBound(p3, p4)  // Đoạn móc neo phải
                    };

                    // ============================================================
                    // BƯỚC 3 (TIẾP THEO): TÍNH TOÁN ĐƯỜNG CURVE CHO THÉP CẠNH NGẮN (PHƯƠNG Y)
                    // ============================================================
                    double hookYFoot = UnitUtils.ConvertToInternalUnits(this.ShortHookLength, UnitTypeId.Millimeters);

                    // Xác định tọa độ giới hạn của thanh thép theo phương Y (Đã trừ lớp bảo vệ 2 đầu móng)
                    double yStart = -widthFoot / 2 + coverFoot;
                    double yEnd = widthFoot / 2 - coverFoot;

                    // Thép ngắn xếp chồng lên trên thép dọc, cao độ Z tịnh tiến thêm đường kính thép dọc (Phương X)
                    double longDiaFoot = UnitUtils.ConvertToInternalUnits(longDia, UnitTypeId.Millimeters);
                    double zBottomShort = zBottom + longDiaFoot;

                    // Vị trí thanh thép ngắn đầu tiên nằm sát mép bảo vệ phương X của móng
                    double xPositionY = -lengthFoot / 2 + coverFoot;

                    // Khởi tạo 4 điểm nút tọa độ tạo dựng hình dáng chữ U ngửa phương Y
                    XYZ q1 = new XYZ(xPositionY, yStart, zBottomShort + hookYFoot); // Đỉnh móc trái
                    XYZ q2 = new XYZ(xPositionY, yStart, zBottomShort);             // Góc vuông dưới trái
                    XYZ q3 = new XYZ(xPositionY, yEnd, zBottomShort);               // Góc vuông dưới phải
                    XYZ q4 = new XYZ(xPositionY, yEnd, zBottomShort + hookYFoot);   // Đỉnh móc phải

                    List<Curve> curvesY = new List<Curve>
                    {
                        Line.CreateBound(q1, q2), // Đoạn móc neo trái
                        Line.CreateBound(q2, q3), // Đoạn thân nằm ngang
                        Line.CreateBound(q3, q4)  // Đoạn móc neo phải
                    };

                    // Thực thi đóng gói Transaction tạm thời 
                    trans.Commit();
                }

                // Tự động đóng form sau khi xử lý thành công
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi thực thi", $"Có lỗi xảy ra trong quá trình bóc tách hình học hoặc tính toán Curve: {ex.Message}");
            }
        }

        #region Hỗ trợ cập nhật giao diện (INotifyPropertyChanged)
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}