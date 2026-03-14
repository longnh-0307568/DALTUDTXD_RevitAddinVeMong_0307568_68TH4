using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using Autodesk.Revit.UI;

namespace AddinVeMong.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private ExternalCommandData _commanData;

        // Thuộc tính quản lý Visibility cho Grid x:Name="InputCentric"
        private Visibility _inputCentricVisibility = Visibility.Visible;
        public Visibility InputCentricVisibility
        {
            get => _inputCentricVisibility;
            set { _inputCentricVisibility = value; OnPropertyChanged(); }
        }

        // Thuộc tính quản lý Visibility cho Grid x:Name="InputEccentric"
        private Visibility _inputEccentricVisibility = Visibility.Collapsed;
        public Visibility InputEccentricVisibility
        {
            get => _inputEccentricVisibility;
            set { _inputEccentricVisibility = value; OnPropertyChanged(); }
        }

        // Commands thay thế cho các hàm Click cũ
        public ICommand CentricInput { get; }
        public ICommand EccentricInput { get; }

        public MainViewModel(ExternalCommandData commandData)
        {
            _commanData = commandData;

            // Logic khi nhấn "Đúng Tâm"
            CentricInput = new RelayCommand(obj =>
            {
                InputCentricVisibility = Visibility.Visible;
                InputEccentricVisibility = Visibility.Collapsed;
            });

            // Logic khi nhấn "Lệch Tâm"
            EccentricInput = new RelayCommand(obj =>
            {
                InputCentricVisibility = Visibility.Collapsed;
                InputEccentricVisibility = Visibility.Visible;
            });
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    // Lớp bổ trợ RelayCommand để Binding lệnh từ XAML
    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        public RelayCommand(Action<object?> execute) => _execute = execute;
        public bool CanExecute(object? parameter) => true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
    }
}