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
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            try
            {
                // Chọn móng trước
                Reference r = uidoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Chọn móng để đặt thép");
                Element host = doc.GetElement(r);

                // Truyền host vào ViewModel
                var vm = new RebarViewModel(commandData, host);
                var view = new AddinVeMong.Views.RebarView { DataContext = vm };

                view.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception) { return Result.Cancelled; }
        }
    }
}