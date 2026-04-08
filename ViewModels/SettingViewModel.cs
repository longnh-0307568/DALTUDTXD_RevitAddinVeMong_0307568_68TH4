namespace AddinVeMong.ViewModels
{
    public class SettingViewModel
    {
        public string TitleText { get; set; }

        public SettingViewModel()
        {
            // Nội dung hiển thị cho Label trong view Cài đặt
            TitleText = "Cấu hình Add-in Thiết kế móng";
        }
    }
}