using AddinVeMong.Commands;
using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    public class ChamferFootingViewModel : INotifyPropertyChanged
    {
        private ExternalCommandData _commandData;

        // --- KÍCH THƯỚC ĐÁY MÓNG ---
        private double _length = 4000;
        public double Length { get => _length; set { _length = value; OnPropertyChanged(); } }

        private double _width = 3000;
        public double Width { get => _width; set { _width = value; OnPropertyChanged(); } }

        // --- KÍCH THƯỚC ĐỈNH (CỔ MÓNG) ---
        private double _topLength = 1200;
        public double TopLength { get => _topLength; set { _topLength = value; OnPropertyChanged(); } }

        private double _topWidth = 1000;
        public double TopWidth { get => _topWidth; set { _topWidth = value; OnPropertyChanged(); } }

        // --- CHIỀU CAO ---
        private double _hBase = 300;
        public double HBase { get => _hBase; set { _hBase = value; OnPropertyChanged(); } }

        private double _hStraight = 300;
        public double HStraight { get => _hStraight; set { _hStraight = value; OnPropertyChanged(); } }

        private double _hChamfer = 500;
        public double HChamfer { get => _hChamfer; set { _hChamfer = value; OnPropertyChanged(); } }

        public ICommand CreateCommand { get; }

        public ChamferFootingViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            CreateCommand = new RelayCommand(ExecuteCreate);
        }

        private void ExecuteCreate(object? obj)
        {
            var window = obj as System.Windows.Window;
            // Gọi logic vẽ móng
            CreateChamferFootingCommand.ExecuteLogic(_commandData, this);
            window?.Close();
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}