using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AddinVeMong.Views;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SettingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Khởi tạo cửa sổ cài đặt
            SettingView window = new SettingView();

            // Gán dữ liệu (ViewModel) cho giao diện
            window.DataContext = new SettingViewModel();

            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}