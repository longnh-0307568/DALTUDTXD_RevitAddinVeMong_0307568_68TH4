using System.Reflection;

namespace AddinVeMong.ViewModels
{
    public class AboutViewModel
    {
        public string ProjectTitle { get; set; }
        public string Description { get; set; }
        public string VersionInfo { get; set; }
        public string MemberPlaceholder { get; set; }

        public AboutViewModel()
        {
            ProjectTitle = "ADD-IN THIẾT KẾ MÓNG ĐƠN";

            // Giới thiệu ngắn gọn dưới 5 dòng
            Description = "Công cụ hỗ trợ kỹ sư tự động hóa quá trình mô hình cốt thép móng đơn trong Revit.\n" +
                          "Chức năng chính: Quản lý thông số hình học, tự động bố trí thép cạnh dài, thép cạnh ngắn,\n" +
                          "thép cổ cột và thép đai. Giúp tăng tốc độ thiết kế.";

            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            VersionInfo = $"Phiên bản: {version}";

            // Placeholder thành viên
            MemberPlaceholder = "Sinh viên 1: Nguyễn Hoàng Long\n" +
                                "Sinh viên 2: Đoàn Quang Ánh\n";
        }
    }
}