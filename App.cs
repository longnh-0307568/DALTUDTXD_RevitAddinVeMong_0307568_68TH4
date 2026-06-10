using System;
using System.Reflection;
using System.Windows.Media.Imaging;
using Autodesk.Revit.UI;

namespace AddinVeMong
{
    public class App : IExternalApplication
    {
        public Result OnStartup(UIControlledApplication application)
        {
            string tabName = "Vẽ Móng";

            application.CreateRibbonTab(tabName);

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            // ĐƯỜNG DẪN COMMAND HỆ THỐNG
            string aboutCommandPath = "AddinVeMong.Commands.AboutCommand";
            string supportCommandPath = "AddinVeMong.Commands.SupportCommand";
            string settingCommandPath = "AddinVeMong.Commands.SettingCommand";

            // ĐƯỜNG DẪN 2 COMMAND MỞ UI
            string showConcentricUIPath = "AddinVeMong.Commands.ShowConcentricWindowCommand";
            string showEccentricUIPath = "AddinVeMong.Commands.ShowEccentricWindowCommand";

            // ĐƯỜNG DẪN COMMAND LOAD FAMILY
            string loadFoundationFamilyPath =
                "AddinVeMong.Commands.LoadFoundationFamilyCommand";

            string loadRebarFamilyPath =
                "AddinVeMong.Commands.LoadRebarFamilyCommand";

            // PANEL GIỚI THIỆU

            RibbonPanel panelAbout =
                application.CreateRibbonPanel(tabName, "Giới thiệu");

            PushButtonData btnAboutData = new PushButtonData(
                "btnAbout",
                "Giới thiệu",
                assemblyPath,
                aboutCommandPath
            );

            PushButton btnAbout =
                panelAbout.AddItem(btnAboutData) as PushButton;

            btnAbout.LargeImage =
                CreateImage(assemblyName, "About.png");

            PushButtonData btnSupportData = new PushButtonData(
                "btnSupport",
                "Hỗ trợ",
                assemblyPath,
                supportCommandPath
            );

            btnSupportData.Image =
                CreateImage(assemblyName, "Help.png");

            PushButtonData btnSettingsData = new PushButtonData(
                "btnSettings",
                "Cài đặt",
                assemblyPath,
                settingCommandPath
            );

            btnSettingsData.Image =
                CreateImage(assemblyName, "Setting.png");

            panelAbout.AddStackedItems(
                btnSupportData,
                btnSettingsData
            );

            // PANEL THÉP

            RibbonPanel panelRebar =
                application.CreateRibbonPanel(tabName, "Thép");

            PulldownButtonData pulldownRebarData =
                new PulldownButtonData(
                    "pdPlaceRebar",
                    "Đặt thép"
                );

            PulldownButton pulldownRebar =
                panelRebar.AddItem(pulldownRebarData) as PulldownButton;

            pulldownRebar.LargeImage =
                CreateImage(assemblyName, "Rebar.png");

            // Móng đúng tâm
            PushButtonData btnConcentricData = new PushButtonData(
                "btnDungTam",
                "Móng đơn đúng tâm",
                assemblyPath,
                showConcentricUIPath
            );

            btnConcentricData.LargeImage =
                CreateImage(assemblyName, "CentricChamfer.png");

            pulldownRebar.AddPushButton(btnConcentricData);

            // Móng lệch tâm
            PushButtonData btnEccentricData = new PushButtonData(
                "btnLechTam",
                "Móng đơn lệch tâm",
                assemblyPath,
                showEccentricUIPath
            );

            btnEccentricData.LargeImage =
                CreateImage(assemblyName, "EccentricChamfer.png");

            pulldownRebar.AddPushButton(btnEccentricData);


            // PANEL THƯ VIỆN
            RibbonPanel panelLibrary =
                application.CreateRibbonPanel(tabName, "Thư viện");

            // Tải family móng
            PushButtonData btnLoadFoundationData = new PushButtonData(
                "btnLoadFoundation",
                "Tải family\nmóng",
                assemblyPath,
                loadFoundationFamilyPath
            );

            btnLoadFoundationData.Image =
                CreateImage(assemblyName, "LoadFoundation.png");

            // Tải family thép
            PushButtonData btnLoadRebarData = new PushButtonData(
                "btnLoadRebar",
                "Tải family\nthép",
                assemblyPath,
                loadRebarFamilyPath
            );

            btnLoadRebarData.Image =
                CreateImage(assemblyName, "LoadRebar.png");

            // Add stacked buttons
            panelLibrary.AddStackedItems(
                btnLoadFoundationData,
                btnLoadRebarData
            );

            return Result.Succeeded;
        }

        private BitmapImage CreateImage(string assemblyName, string imageName)
        {
            try
            {
                string uriPath =
                    $"pack://application:,,,/{assemblyName};component/Resources/Images/{imageName}";

                return new BitmapImage(new Uri(uriPath));
            }
            catch
            {
                return null;
            }
        }

        public Result OnShutdown(UIControlledApplication application)
            => Result.Succeeded;
    }
}