using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;

namespace AddinVeMong
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            // 1. Tạo Ribbon Tab
            string tabName = "THIẾT KẾ MÓNG";
            application.CreateRibbonTab(tabName);

            // 2. Lấy thông tin Assembly
            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name; // Sẽ là "AddinVeMong"
            string testCommand = "AddinVeMong.TestCommand";

            // --- PANEL 1: GIỚI THIỆU ---
            RibbonPanel panelGioiThieu = application.CreateRibbonPanel(tabName, "Giới thiệu");

            // Nút Giới thiệu (Nút lớn)
            PushButtonData btnAboutData = new PushButtonData("btnAbout", "Giới thiệu", assemblyPath, testCommand);
            PushButton btnAbout = panelGioiThieu.AddItem(btnAboutData) as PushButton;
            btnAbout.LargeImage = CreateImage(assemblyName, "About.png");

            // Nút Hỗ trợ (Nút nhỏ trong Stack)
            PushButtonData btnSupportData = new PushButtonData("btnSupport", "Hỗ trợ", assemblyPath, testCommand);
            btnSupportData.Image = CreateImage(assemblyName, "Help.png"); // File ảnh của bạn là Help.png hoặc Support.png

            // Nút Cài đặt (Nút nhỏ trong Stack)
            PushButtonData btnSettingsData = new PushButtonData("btnSettings", "Cài đặt", assemblyPath, testCommand);
            btnSettingsData.Image = CreateImage(assemblyName, "Setting.png");

            panelGioiThieu.AddStackedItems(btnSupportData, btnSettingsData);


            // --- PANEL 2: THÉP ---
            RibbonPanel panelThep = application.CreateRibbonPanel(tabName, "Thép");

            PushButtonData btnDatThepData = new PushButtonData("btnPlaceRebar", "Đặt thép", assemblyPath, testCommand);
            PushButton btnDatThep = panelThep.AddItem(btnDatThepData) as PushButton;
            btnDatThep.LargeImage = CreateImage(assemblyName, "Draw.png"); // Ví dụ dùng ảnh Draw.png có trong csproj của bạn

            return Result.Succeeded;
        }

        /// <summary>
        /// Hàm tạo BitmapImage từ Resource bằng Pack URI
        /// </summary>
        private BitmapImage CreateImage(string assemblyName, string imageName)
        {
            try
            {
                // Cấu trúc URI chuẩn để lấy Resource từ file DLL
                string uriPath = $"pack://application:,,,/{assemblyName};component/Assets/Images/{imageName}";
                return new BitmapImage(new Uri(uriPath));
            }
            catch
            {
                return null;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }
    }

    [Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Revit Addin Test", "Nút hoạt động bình thường!");
            return Result.Succeeded;
        }
    }
}