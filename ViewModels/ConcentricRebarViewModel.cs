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
                // Toàn bộ logic lấy đối tượng móng được chọn, tính toán hình học 
                // và gọi lệnh vẽ Rebar của Revit sẽ được viết ở đây.

                // Tạm thời hiển thị thông báo test nhận dữ liệu
                TaskDialog.Show("Revit API", $"Đang chuẩn bị vẽ móng đúng tâm với lớp bảo vệ: {Cover}mm.");

                // Sau khi vẽ thép thành công, tiến hành đóng cửa sổ giao diện
                if (parameter is Window window)
                {
                    window.DialogResult = true;
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                TaskDialog.Show("Lỗi", $"Có lỗi xảy ra trong quá trình vẽ: {ex.Message}");
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