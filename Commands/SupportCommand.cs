using AddinVeMong.ViewModels;
using AddinVeMong.Views;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class SupportCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            SupportView window = new SupportView();
            window.DataContext = new SupportViewModel();
            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}