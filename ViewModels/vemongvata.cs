using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using AddinVeMong.Views;
using AddinVeMong.ViewModels;

namespace vemongvata
{
    [Transaction(TransactionMode.Manual)]
    public class Cmd_CreateChamferFooting : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            try
            {
                // Khởi tạo ViewModel và View
                var viewModel = new FootingViewModel(commandData);
                var view = new FootingView();
                
                // Binding dữ liệu
                view.DataContext = viewModel;

                // Hiển thị cửa sổ
                view.ShowDialog();

                return Result.Succeeded;
            }
            catch (Exception ex)
            {
                message = ex.Message;
                return Result.Failed;
            }
        }
    }
}