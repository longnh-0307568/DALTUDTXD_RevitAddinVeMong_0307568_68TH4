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
            SettingView window = new SettingView();

            // Ép cửa sổ Setting nhận theme hiện tại từ ThemeManager
            window.Resources.MergedDictionaries.Add(Helpers.ThemeManager.CurrentThemeResource);

            window.DataContext = new SettingViewModel();
            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}