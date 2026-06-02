using System;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using AddinVeMong.Views;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Commands
{
    //Bắt buộc phải có attribute này để cho phép add-in can thiệp, chỉnh sửa mô hình Revit
    [Transaction(TransactionMode.Manual)]
    public class ShowConcentricWindowCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Khởi tạo giao diện cửa sổ (View) mà chúng ta vừa thiết kế xaml
                ConcentricRebarView window = new ConcentricRebarView();

                // Khởi tạo bộ não xử lý (ViewModel), truyền commandData (ngữ cảnh Revit) vào
                ConcentricRebarViewModel viewModel = new ConcentricRebarViewModel(commandData);

                // 
                // Nhờ dòng này, các thuộc tính {Binding Cover}, {Binding LongSpacing} trên XAML mới hiểu và hoạt động được
                window.DataContext = viewModel;

                //Hiển thị cửa sổ dưới dạng Dialog (khóa màn hình nền Revit cho đến khi tương tác xong)
                window.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                // Nếu có lỗi đột xuất xảy ra, gán tin nhắn lỗi để Revit thông báo cho người dùng
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}