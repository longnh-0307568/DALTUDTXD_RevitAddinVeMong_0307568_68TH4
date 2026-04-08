using System.ComponentModel;
using System.Runtime.CompilerServices;
using Autodesk.Revit.UI;

namespace AddinVeMong.ViewModels
{
    public class RebarViewModel : INotifyPropertyChanged
    {
        private ExternalCommandData _commandData;
        private string _infoText;

        public RebarViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            InfoText = "Cấu hình thép cho móng";
        }

        public string InfoText
        {
            get => _infoText;
            set { _infoText = value; OnPropertyChanged(); }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}