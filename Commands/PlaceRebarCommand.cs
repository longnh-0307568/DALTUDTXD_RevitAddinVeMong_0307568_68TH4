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
                // 1. Chỉnh chế độ chọn nhiều móng
                IList<Reference> refs = uidoc.Selection.PickObjects(
                    Autodesk.Revit.UI.Selection.ObjectType.Element,
                    "Chọn các móng để đặt thép hàng loạt (Nhấn Finish trên thanh công cụ khi chọn xong)");

                List<Element> hosts = new List<Element>();
                foreach (Reference r in refs)
                {
                    Element el = doc.GetElement(r);
                    // Kiểm tra xem có phải là móng không (Foundation)
                    if (el.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFoundation)
                    {
                        hosts.Add(el);
                    }
                }

                if (hosts.Count == 0) return Result.Cancelled;

                // 2. Truyền danh sách host vào ViewModel
                var vm = new RebarViewModel(commandData, hosts);
                var view = new AddinVeMong.Views.RebarView { DataContext = vm };

                view.ShowDialog();
                return Result.Succeeded;
            }
            catch (Exception) { return Result.Cancelled; }
        }
    }
}