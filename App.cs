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
            string tabName = "Vẽ Móng";

            application.CreateRibbonTab(tabName);

            string assemblyPath = Assembly.GetExecutingAssembly().Location;
            string assemblyName = Assembly.GetExecutingAssembly().GetName().Name;

            // ĐƯỜNG DẪN COMMAND MỚI (Lưu ý có chữ .Commands)
            string aboutCommandPath = "AddinVeMong.Commands.AboutCommand";
            string supportCommandPath = "AddinVeMong.Commands.SupportCommand";
            string settingCommandPath = "AddinVeMong.Commands.SettingCommand";
            string placeRebarPath = "AddinVeMong.Commands.PlaceRebarCommand";
            string createChamferFootingPath = "AddinVeMong.Commands.ChamferFootingCommand";
            string createEccentricChamferFootingPath = "AddinVeMong.Commands.CreateEccentricFootingCommand";
            string testCommandPath = "AddinVeMong.TestCommand";

            // Tạo pannel Giới thiệu
            RibbonPanel panelAbout = application.CreateRibbonPanel(tabName, "Giới thiệu");
            // Thêm nút 'Giới thiệu' vào pannel
            PushButtonData btnAboutData = new PushButtonData("btnAbout", "Giới thiệu", assemblyPath, aboutCommandPath);
            PushButton btnAbout = panelAbout.AddItem(btnAboutData) as PushButton;
            btnAbout.LargeImage = CreateImage(assemblyName, "About.png");

            // Thêm nút 'Hỗ trợ'
            PushButtonData btnSupportData = new PushButtonData("btnSupport", "Hỗ trợ", assemblyPath, supportCommandPath);
            btnSupportData.Image = CreateImage(assemblyName, "Help.png");
            // Thêm nút 'Cài đặt'
            PushButtonData btnSettingsData = new PushButtonData("btnSettings", "Cài đặt", assemblyPath, settingCommandPath);
            btnSettingsData.Image = CreateImage(assemblyName, "Setting.png");

            panelAbout.AddStackedItems(btnSupportData, btnSettingsData); // Thêm 2 nút kiểu Stack

            // Tạo pannel 'Thép'
            RibbonPanel panelRebar = application.CreateRibbonPanel(tabName, "Thép");
            PushButtonData btnPlaceRebarData = new PushButtonData("btnPlaceRebar", "Đặt thép", assemblyPath, placeRebarPath);
            PushButton btnPlaceRebar = panelRebar.AddItem(btnPlaceRebarData) as PushButton;
            btnPlaceRebar.LargeImage = CreateImage(assemblyName, "Rebar.png");

            RibbonPanel panelFooting = application.CreateRibbonPanel(tabName, "Móng");
            // Tạo PulldownButton
            PulldownButtonData pullDownFootingData = new PulldownButtonData("pullDownFooting", "Móng đơn");
            PulldownButton pullDownFooting = panelFooting.AddItem(pullDownFootingData) as PulldownButton;
            pullDownFooting.LargeImage = CreateImage(assemblyName, "Footing.png");

            // Thêm Item "Móng đúng tâm" vào trong menu thả xuống
            PushButtonData btnChamferFootingData = new PushButtonData("btnChamferFooting", "Móng đúng tâm", assemblyPath, createChamferFootingPath);
            btnChamferFootingData.ToolTip = "Vẽ móng đơn vát cạnh bằng cách chọn điểm";
            btnChamferFootingData.LargeImage = CreateImage(assemblyName, "CentricChamfer.png"); // Icon nhỏ cho item
            pullDownFooting.AddPushButton(btnChamferFootingData);

            // Thêm item "Móng lệch tâm" vào menu thả xuống
            PushButtonData btnEccentricChamferFootingData = new PushButtonData("btnEccentricChamferFooting", "Móng lệch tâm", assemblyPath, createEccentricChamferFootingPath);
            btnEccentricChamferFootingData.ToolTip = "Vẽ móng đon vát lệch tâm bằng cách chọn điểm";
            btnEccentricChamferFootingData.LargeImage = CreateImage(assemblyName, "EccentricChamfer.png");
            pullDownFooting.AddPushButton(btnEccentricChamferFootingData);

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