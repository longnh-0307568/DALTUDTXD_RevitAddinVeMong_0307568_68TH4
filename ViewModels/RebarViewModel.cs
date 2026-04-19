using AddinVeMong.Commands;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    public class RebarViewModel : INotifyPropertyChanged
    {
        private ExternalCommandData _commandData;

        // 1. THÊM THUỘC TÍNH SELECTEDHOST ĐỂ REBARLOGIC TRUY CẬP
        private Element _selectedHost;
        public Element SelectedHost
        {
            get => _selectedHost;
            set { _selectedHost = value; OnPropertyChanged(); }
        }

        // 2. CẬP NHẬT CONSTRUCTOR ĐỂ NHẬN MÓNG (HOST) TỪ COMMAND
        public RebarViewModel(ExternalCommandData commandData, Element host)
        {
            _commandData = commandData;
            SelectedHost = host; // Gán móng được chọn vào thuộc tính
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

        private int _shortDiameter = 12;
        public int ShortDiameter
        {
            get => _shortDiameter;
            set { _shortDiameter = value; OnPropertyChanged(); }
        }

        // --- THÉP CHỜ CỘT ---
        private int _starterDiameter = 16;
        public int StarterDiameter
        {
            get => _starterDiameter;
            set { _starterDiameter = value; OnPropertyChanged(); }
        }

        private int _starterNX = 2;
        public int StarterNX
        {
            get => _starterNX;
            set { _starterNX = value; OnPropertyChanged(); }
        }

        private int _starterNY = 2;
        public int StarterNY
        {
            get => _starterNY;
            set { _starterNY = value; OnPropertyChanged(); }
        }

        private int _starterLength = 600;
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

            // 3. GỌI LOGIC VẼ THÉP TỪ REBARLOGIC (Giữ nguyên BasisY bên trong đó)
            // Truyền 'this' (chính là ViewModel này) vào để Logic lấy được LongCount, LongDiameter...
            RebarLogic.ExecuteDrawRebar(_commandData, this);

            window?.Close();
        }

        // --- INotifyPropertyChanged ---
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}