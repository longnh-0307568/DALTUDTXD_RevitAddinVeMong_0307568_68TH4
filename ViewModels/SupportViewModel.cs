namespace AddinVeMong.ViewModels
{
    public class SupportViewModel
    {
        public string TutorialTitle { get; set; }
        public string Content { get; set; }

        public SupportViewModel()
        {
            TutorialTitle = "HƯỚNG DẪN SỬ DỤNG";
            Content = "- Bước 1: Chọn mặt bằng kết cấu.\n" +
                      "- Bước 2: Nhấn nút 'Đặt thép'.\n" +
                      "- Bước 3: Nhập thông số và nhấn 'Vẽ'.";
        }
    }
}