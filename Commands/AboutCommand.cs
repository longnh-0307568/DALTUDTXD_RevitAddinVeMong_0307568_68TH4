using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AddinVeMong.Views;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class AboutCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Khởi tạo giao diện và dữ liệu
            AboutView window = new AboutView();
            window.DataContext = new AboutViewModel();

            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}