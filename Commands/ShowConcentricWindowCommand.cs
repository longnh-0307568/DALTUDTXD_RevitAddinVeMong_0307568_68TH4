using AddinVeMong.ViewModels;
using AddinVeMong.Views;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection; // Thư viện này để dùng được PickObjects

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ShowConcentricWindowCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // tiếng việt mặc định
            // Giúp form thép tự động đọc file Resource.resx gốc (Tiếng Việt) ngay lần đầu mở lên

            string cultureCode = SettingViewModel.CurrentLanguageSettings == "English" ? "en" : "vi";
            var culture = new System.Globalization.CultureInfo(cultureCode);
            System.Threading.Thread.CurrentThread.CurrentCulture = culture;
            System.Threading.Thread.CurrentThread.CurrentUICulture = culture;

            UIDocument uiDoc = commandData.Application.ActiveUIDocument;
            Document doc = uiDoc.Document;

            try
            {
                // 1. Tạo bộ lọc chỉ cho phép quét chọn Structural Foundation (Móng)
                FootingSelectionFilter filter = new FootingSelectionFilter();

                // Yêu cầu người dùng chọn móng ngoài màn hình Revit trước (Không hiện thông báo popup phiền phức)
                // Người dùng có thể quét chuột chọn nhiều móng, sau đó bấm nút "Finish" ở góc trên bên trái Revit
                IList<Reference> pickedRefs = uiDoc.Selection.PickObjects(
                    ObjectType.Element,
                    filter,
                    "Vui lòng quét chọn các cấu kiện móng đơn, sau đó nhấn Finish ở góc trên bên trái màn hình!");

                List<Element> selectedFootings = new List<Element>();
                foreach (Reference r in pickedRefs)
                {
                    Element elem = doc.GetElement(r);
                    if (elem != null)
                    {
                        selectedFootings.Add(elem);
                    }
                }

                // Nếu người dùng không chọn cấu kiện nào hoặc bấm Finish khi chưa chọn
                if (selectedFootings.Count == 0)
                {
                    TaskDialog.Show("Thông báo", "Bạn chưa chọn cấu kiện móng nào!");
                    return Result.Cancelled;
                }

                // Khởi tạo giao diện cửa sổ (View) 
                ConcentricRebarView window = new ConcentricRebarView();


                // Ép cửa sổ Form Thép nhận theme hiện tại từ ThemeManager (Ghi đè hoàn toàn LightTheme trong XAML)
                window.Resources.MergedDictionaries.Add(Helpers.ThemeManager.CurrentThemeResource);

                // Khởi tạo bộ não xử lý (ViewModel)
                ConcentricRebarViewModel viewModel = new ConcentricRebarViewModel(commandData, selectedFootings);
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

    // Bộ lọc phụ trợ nằm ngay trong file Command để lọc cấu kiện móng đơn
    public class FootingSelectionFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            return elem.Category != null && elem.Category.Id.IntegerValue == (int)BuiltInCategory.OST_StructuralFoundation;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return false;
        }
    }
}