using Microsoft.Win32;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class LoadFoundationFamilyCommand : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIDocument uidoc = commandData.Application.ActiveUIDocument;
            Document doc = uidoc.Document;

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Title = "Chọn Family Móng";
            dialog.Filter = "Revit Family (*.rfa)|*.rfa";

            dialog.InitialDirectory =
                @"C:\ProgramData\Autodesk\RVT 2025\Libraries\English\US\Structural Foundations";

            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                string familyPath = dialog.FileName;

                using (Transaction trans = new Transaction(doc, "Load Foundation Family"))
                {
                    trans.Start();

                    bool loaded = doc.LoadFamily(familyPath);

                    trans.Commit();

                    if (loaded)
                    {
                        TaskDialog.Show("Thông báo", "Load family thành công.");
                    }
                    else
                    {
                        TaskDialog.Show("Thông báo", "Family đã tồn tại hoặc load thất bại.");
                    }
                }
            }

            return Result.Succeeded;
        }
    }
}