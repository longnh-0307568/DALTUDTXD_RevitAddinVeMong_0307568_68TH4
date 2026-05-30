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

            // ĐƯỜNG DẪN COMMAND
            string aboutCommandPath = "AddinVeMong.Commands.AboutCommand";
            string supportCommandPath = "AddinVeMong.Commands.SupportCommand";
            string settingCommandPath = "AddinVeMong.Commands.SettingCommand";

            // ĐƯỜNG DẪN 2 COMMAND MỚI CHO MÓNG ĐÚNG TÂM VÀ LỆCH TÂM
            string placeRebarConcentricPath = "AddinVeMong.Commands.PlaceRebarConcentricCommand";
            string placeRebarEccentricPath = "AddinVeMong.Commands.PlaceRebarEccentricCommand";

            // Tạo pannel Giới thiệu
            RibbonPanel panelAbout = application.CreateRibbonPanel(tabName, "Giới thiệu");
            // Thêm nút 'Giới thiệu' vào pannel
            PushButtonData btnAboutData = new PushButtonData("btnAbout", "Giới thiệu", assemblyPath, aboutCommandPath);
            PushButton btnAbout = panelAbout.AddItem(btnAboutData) as PushButton;
            btnAbout.LargeImage = CreateImage(assemblyName, "About.png");

            // Nút 'Hỗ trợ'
            PushButtonData btnSupportData = new PushButtonData("btnSupport", "Hỗ trợ", assemblyPath, supportCommandPath);
            btnSupportData.Image = CreateImage(assemblyName, "Help.png");

            // Nút 'Cài đặt'
            PushButtonData btnSettingsData = new PushButtonData("btnSettings", "Cài đặt", assemblyPath, settingCommandPath);
            btnSettingsData.Image = CreateImage(assemblyName, "Setting.png");

            // Thêm 2 nút kiểu Stack
            panelAbout.AddStackedItems(btnSupportData, btnSettingsData);

            // CẬP NHẬT PANEL 'THÉP' SỬ DỤNG PULLDOWN BUTTON
            RibbonPanel panelRebar = application.CreateRibbonPanel(tabName, "Thép");

            // 1. Khởi tạo nút thả xuống (Pulldown Button) đóng vai trò là menu cha
            PulldownButtonData pulldownRebarData = new PulldownButtonData("pdPlaceRebar", "Đặt thép");
            PulldownButton pulldownRebar = panelRebar.AddItem(pulldownRebarData) as PulldownButton;
            pulldownRebar.LargeImage = CreateImage(assemblyName, "Rebar.png"); // Icon chính hiển thị trên Ribbon

            // 2. Thêm nút con thứ nhất: Móng đơn đúng tâm
            PushButtonData btnConcentricData = new PushButtonData(
                "btnDungTam",
                "Móng đơn đúng tâm",
                assemblyPath,
                placeRebarConcentricPath
            );
            btnConcentricData.LargeImage = CreateImage(assemblyName, "Rebar.png"); // Bạn có thể đổi tên icon riêng nếu muốn
            pulldownRebar.AddPushButton(btnConcentricData);

            // 3. Thêm nút con thứ hai: Móng đơn lệch tâm
            PushButtonData btnEccentricData = new PushButtonData(
                "btnLechTam",
                "Móng đơn lệch tâm",
                assemblyPath,
                placeRebarEccentricPath
            );
            btnEccentricData.LargeImage = CreateImage(assemblyName, "Rebar.png"); // Bạn có thể đổi tên icon riêng nếu muốn
            pulldownRebar.AddPushButton(btnEccentricData);

            return Result.Succeeded;
        }

        // Hỗ trợ tạo đường dẫn để chỉ cần nhập tên icon
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
}