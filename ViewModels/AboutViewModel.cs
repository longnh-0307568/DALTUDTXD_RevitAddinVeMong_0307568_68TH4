using System;
using System.Reflection;

namespace AddinVeMong.ViewModels
{
    public class AboutViewModel
    {
        public string DisplayText { get; set; }

        public AboutViewModel()
        {
            string version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "1.0.0";
            string userName = Environment.UserName;
            string computerName = Environment.MachineName;

            DisplayText = $"PROJECT: ADD-IN THIẾT KẾ MÓNG ĐƠN VÁT\n" +
              $"Phiên bản: {version} (Beta)\n" +
              $"Sản phẩm thuộc đồ án môn học: Lập trình ứng dụng trong xây dựng\n\n" +
              $"Thực hiện bởi: Nhóm\n" +
              $"- Sinh viên 1: Nguyễn Hoàng Long\n" +
              $"- Sinh viên 2: Đoàn Quang Ánh\n" +
              $"- Sinh viên 3: Nguyễn Hữu Hùng\n" +
              $"Giảng viên hướng dẫn: Thầy Nguyễn Văn Hải\n" +
              $"------------------------------------------\n" +
              $"Cảm ơn thầy/cô đã sử dụng Add-in trên thiết bị: {computerName}";
        }
    }
}