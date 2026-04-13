using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using AddinVeMong.Commands;

namespace AddinVeMong.ViewModels
{
    public class RebarViewModel : INotifyPropertyChanged
    {
        // Dữ liệu chỉ để Binding với UI
        public int LongCount { get; set; } = 8;
        public int LongDiameter { get; set; } = 14;
        public int ShortCount { get; set; } = 6;
        public int ShortDiameter { get; set; } = 14;
        public int StarterCount { get; set; } = 4;
        public int StarterDiameter { get; set; } = 18;

        public ICommand DrawRebarCommand { get; }

        public RebarViewModel()
        {
            // Command bây giờ sẽ gọi một lớp xử lý riêng
            DrawRebarCommand = new RelayCommand(ExecuteDraw);
        }

        private void ExecuteDraw(object? obj)
        {
            // Chỉ cần viết
            // RebarLogic.Run(this); 
            // (Trong đó RebarLogic là file trong thư mục Commands)
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}