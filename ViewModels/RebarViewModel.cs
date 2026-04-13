using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AddinVeMong.Commands;
using Autodesk.Revit.UI;

namespace AddinVeMong.ViewModels
{
    public class RebarViewModel : INotifyPropertyChanged
    {
        private ExternalCommandData _commandData;

        public RebarViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DrawRebarCommand = new RelayCommand(ExecuteDraw);
        }

        // --- THÉP CẠNH DÀI ---
        private int _longCount = 8;
        public int LongCount
        {
            get => _longCount;
            set { _longCount = value; OnPropertyChanged(); }
        }

        private int _longDiameter = 14;
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

        // --- THÉP CẠNH NGẮN ---
        private int _shortCount = 6;
        public int ShortCount
        {
            get => _shortCount;
            set { _shortCount = value; OnPropertyChanged(); }
        }

        private int _shortDiameter = 14;
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

        // --- THÉP CHỜ CỘT (STARTER BARS) ---
        private int _starterDiameter = 18;
        public int StarterDiameter
        {
            get => _starterDiameter;
            set { _starterDiameter = value; OnPropertyChanged(); }
        }

        private int _starterNX = 2; // Số thanh theo phương X
        public int StarterNX
        {
            get => _starterNX;
            set { _starterNX = value; OnPropertyChanged(); }
        }

        private int _starterNY = 2; // Số thanh theo phương Y
        public int StarterNY
        {
            get => _starterNY;
            set { _starterNY = value; OnPropertyChanged(); }
        }

        private int _starterLength = 600; // Chiều dài đoạn chờ nhô lên
        public int StarterLength
        {
            get => _starterLength;
            set { _starterLength = value; OnPropertyChanged(); }
        }

        // --- COMMAND ---
        public ICommand DrawRebarCommand { get; }

        private void ExecuteDraw(object? obj)
        {
            var window = obj as System.Windows.Window;

            // Ở đây bạn sẽ gọi lớp Logic vẽ thép trong thư mục Commands
            // Ví dụ: RebarLogic.Execute(_commandData, this);

            TaskDialog.Show("Thông báo", "Đang chuyển dữ liệu sang logic vẽ thép...");

            window?.Close();
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}