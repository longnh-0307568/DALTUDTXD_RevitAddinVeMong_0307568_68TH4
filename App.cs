using AddinVeMong.Views;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddinVeMong
{
    [Transaction(TransactionMode.Manual)]
    public class App : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // Khởi tạo cửa sổ từ thư mục Views
            // Chúng ta truyền commandData vào để sau này lấy Document vẽ móng
            MainWindowView mainView = new (commandData);

            // Hiển thị cửa sổ
            mainView.ShowDialog();

            return Result.Succeeded;
        }
    }
}
