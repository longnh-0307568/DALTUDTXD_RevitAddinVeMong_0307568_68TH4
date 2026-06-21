using AddinVeMong.Helpers; // Đảm bảo namespace chứa ThemeManager
using AddinVeMong.ViewModels;
using AddinVeMong.Views;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class AboutCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            AboutView window = new AboutView();

            // ĐỒNG BỘ THEME: Lấy theme hiện tại từ ThemeManager bơm vào Window
            window.Resources.MergedDictionaries.Add(ThemeManager.CurrentThemeResource);

            // Gán ViewModel
            window.DataContext = new AboutViewModel();

            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}