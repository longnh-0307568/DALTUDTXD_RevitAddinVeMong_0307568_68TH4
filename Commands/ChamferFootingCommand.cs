using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;

namespace AddinVeMong.Commands
{
    [Transaction(TransactionMode.Manual)]
    public class ChamferFootingCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            // 1. Khởi tạo View
            var window = new AddinVeMong.Views.ChamferFootingView();

            // 2. Khởi tạo ViewModel và truyền commandData
            var vm = new AddinVeMong.ViewModels.ChamferFootingViewModel(commandData);

            // 3. Kết nối
            window.DataContext = vm;

            window.ShowDialog();
            return Result.Succeeded;
        }
    }
}