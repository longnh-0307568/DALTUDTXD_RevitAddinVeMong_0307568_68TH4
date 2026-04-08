using Autodesk.Revit.UI;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace AddinVeMong.ViewModels
{
    public class RebarViewModel : INotifyPropertyChanged
    {
        private ExternalCommandData _commandData;

        // Thép phương dài
        public int LongCount { get; set; } = 8;
        public int LongDiameter { get; set; } = 14;

        // Thép phương ngắn
        public int ShortCount { get; set; } = 6;
        public int ShortDiameter { get; set; } = 14;

        // Thép chờ cột
        public int StarterCount { get; set; } = 4;
        public int StarterDiameter { get; set; } = 18;

        public ICommand DrawRebarCommand { get; }

        public RebarViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DrawRebarCommand = new RelayCommand(ExecuteDraw);
        }

        private void ExecuteDraw(object? obj)
        {
            // Lấy đối tượng đang chọn trong Revit
            var uiDoc = _commandData.Application.ActiveUIDocument;
            var selectedIds = uiDoc.Selection.GetElementIds();

            if (selectedIds.Count == 0)
            {
                TaskDialog.Show("Lỗi", "Vui lòng chọn ít nhất 1 cái móng trước khi bấm nút!");
                return;
            }

            // Gọi Transaction để vẽ thép (Phần này ta sẽ viết tiếp)

            TaskDialog.Show("Revit", $"Đang vẽ cho {selectedIds.Count} móng...");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}