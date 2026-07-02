namespace AddinVeMong.ViewModels
{
    public class SupportViewModel
    {
        public string TutorialTitle { get; set; }
        public string Content { get; set; }

        public SupportViewModel()
        {
            TutorialTitle = "HƯỚNG DẪN SỬ DỤNG";

            // Sử dụng ký tự xuống dòng \n và ký tự escape \" cho các dấu ngoặc kép
            Content = "Bước 1: Chọn kiểu móng từ nút \"Đặt thép\" trên panel Thép\n" +
                      "Bước 2: Quét chọn móng, sau đó bấm Finish\n" +
                      "Bước 3: Giao diện mở ra, nhập các thông tin tương ứng\n" +
                      "Bước 4: Bấm \"Đặt thép\" để xem kết quả";
        }
    }
}