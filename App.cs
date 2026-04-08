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
            string tabName = "THIẾT KẾ MÓNG";
            application.CreateRibbonTab(tabName);

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            // ĐƯỜNG DẪN COMMAND MỚI (Lưu ý có chữ .Commands)
            string aboutCommandPath = "AddinVeMong.Commands.AboutCommand";
            string supportCommandPath = "AddinVeMong.Commands.SupportCommand";
            string settingCommandPath = "AddinVeMong.Commands.SettingCommand";
            string placeRebarPath = "AddinVeMong.Commands.PlaceRebarCommand";
            string testCommandPath = "AddinVeMong.TestCommand";

            RibbonPanel panelAbout = application.CreateRibbonPanel(tabName, "Giới thiệu");
            PushButtonData btnAboutData = new PushButtonData("btnAbout", "Giới thiệu", assemblyPath, aboutCommandPath);
            PushButton btnAbout = panelAbout.AddItem(btnAboutData) as PushButton;
            btnAbout.LargeImage = CreateImage(assemblyName, "About.png");

            PushButtonData btnSupportData = new PushButtonData("btnSupport", "Hỗ trợ", assemblyPath, supportCommandPath);
            btnSupportData.Image = CreateImage(assemblyName, "Help.png");

            PushButtonData btnSettingsData = new PushButtonData("btnSettings", "Cài đặt", assemblyPath, settingCommandPath);
            btnSettingsData.Image = CreateImage(assemblyName, "Setting.png");

            panelAbout.AddStackedItems(btnSupportData, btnSettingsData);

            RibbonPanel panelRebar = application.CreateRibbonPanel(tabName, "Thép");
            PushButtonData btnPlaceRebarData = new PushButtonData("btnPlaceRebar", "Đặt thép", assemblyPath, placeRebarPath);
            PushButton btnPlaceRebar = panelRebar.AddItem(btnPlaceRebarData) as PushButton;
            btnPlaceRebar.LargeImage = CreateImage(assemblyName, "Rebar.png");
            return Result.Succeeded;
        }

        private BitmapImage CreateImage(string assemblyName, string imageName)
        {
            try
            {
                string uriPath = $"pack://application:,,,/{assemblyName};component/Assets/Images/{imageName}";
                return new BitmapImage(new Uri(uriPath));
            }
            catch { return null; }
        }

        public Result OnShutdown(UIControlledApplication application) => Result.Succeeded;
    }

    [Transaction(TransactionMode.Manual)]
    public class TestCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            TaskDialog.Show("Revit Addin Test", "Nút này chưa có Popup riêng!");
            return Result.Succeeded;
        }
    }
}