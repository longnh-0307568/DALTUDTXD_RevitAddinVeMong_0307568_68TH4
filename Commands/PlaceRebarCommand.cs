using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AddinVeMong.Views;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class PlaceRebarCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            RebarView window = new RebarView();
            window.DataContext = new RebarViewModel(commandData); // Kết nối với ViewModel mới

            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}