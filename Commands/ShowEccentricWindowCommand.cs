using AddinVeMong.ViewModels;
using AddinVeMong.Views;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ShowEccentricWindowCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Thiết lập ngôn ngữ
            string cultureCode = SettingViewModel.CurrentLanguageSettings == "English" ? "en" : "vi";
            var culture = new System.Globalization.CultureInfo(cultureCode);
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // 1. Tạo bộ lọc chỉ cho phép quét chọn Structural Foundation
                FootingSelectionFilter filter = new FootingSelectionFilter();

                // 2. Yêu cầu người dùng chọn móng
                IList<Reference> pickedRefs = uiDoc.Selection.PickObjects(
                    ObjectType.Element,
                    filter,
                    "Vui lòng quét chọn các cấu kiện móng lệch tâm, sau đó nhấn Finish ở góc trên bên trái màn hình!");

                List<Element> selectedFootings = new List<Element>();
                foreach (Reference r in pickedRefs)
                {
                    Element elem = doc.GetElement(r);
                    if (elem != null) selectedFootings.Add(elem);
                }

                if (selectedFootings.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Bạn chưa chọn cấu kiện móng nào!");
                    return Result.Cancelled;
                }

                // 3. Khởi tạo View và truyền dữ liệu
                // Khởi tạo ViewModel trước
                EccentricRebarViewModel viewModel = new EccentricRebarViewModel(commandData, selectedFootings);

                // 2. Khởi tạo giao diện cửa sổ (View) và truyền trực tiếp viewModel vào trong ngoặc
                EccentricRebarView window = new EccentricRebarView(viewModel);

                // Ép cửa sổ Form Thép nhận theme hiện tại từ ThemeManager
                window.Resources.MergedDictionaries.Add(Helpers.ThemeManager.CurrentThemeResource);

                // (Dòng này có thể giữ lại hoặc bỏ vì bên trong hàm khởi tạo của View đã tự gán DataContext rồi)
                window.DataContext = viewModel;

                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                return Result.Cancelled;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}