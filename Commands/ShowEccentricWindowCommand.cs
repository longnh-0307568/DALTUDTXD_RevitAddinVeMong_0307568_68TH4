using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using AddinVeMong.Views;
using AddinVeMong.ViewModels;

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
                EccentricRebarView window = new EccentricRebarView();

                // Gán theme
                window.Resources.MergedDictionaries.Add(Helpers.ThemeManager.CurrentThemeResource);

                // Khởi tạo ViewModel với danh sách móng đã chọn
                EccentricRebarViewModel viewModel = new EccentricRebarViewModel(commandData, selectedFootings);
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