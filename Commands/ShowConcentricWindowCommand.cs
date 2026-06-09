using System;
using System.Collections.Generic;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection; // Thư viện này để dùng được PickObjects
using AddinVeMong.Views;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ShowConcentricWindowCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
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
                    Element elem = doc.GetElement(r.ElementId);
                    if (elem != null) selectedFootings.Add(elem);
                }

                // Nếu người dùng không chọn móng nào, hủy lệnh
                if (selectedFootings.Count == 0) return Result.Cancelled;


                // 2. CHỌN XONG MÓNG MỚI HIỆN WINDOW
                // Khởi tạo giao diện cửa sổ (View) 
                ConcentricRebarView window = new ConcentricRebarView();

                // Khởi tạo bộ não xử lý (ViewModel), TRUYỀN THÊM danh sách móng đã chọn vào đây để sửa lỗi argument
                ConcentricRebarViewModel viewModel = new ConcentricRebarViewModel(commandData, selectedFootings);

                // Gán DataContext để Binding hoạt động
                window.DataContext = viewModel;

                // Hiển thị cửa sổ dưới dạng Dialog
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Autodesk.Revit.Exceptions.OperationCanceledException)
            {
                // Xử lý êm đẹp trường hợp người dùng bấm ESC hoặc Cancel khi đang chọn móng, không báo lỗi bậy
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