using System;
using System.Windows;

namespace AddinVeMong.Helpers
{
    public static class ThemeManager
    {
        private static ResourceDictionary _currentThemeResource;

        // Thuộc tính tĩnh lưu giữ ResourceDictionary đang hoạt động
        public static ResourceDictionary CurrentThemeResource
        {
            get
            {
                // Nếu chưa được khởi tạo lần nào, mặc định nạp LightTheme
                if (_currentThemeResource == null)
                {
                    UpdateTheme("Sáng (Light)");
                }
                return _currentThemeResource;
            }
        }

        public static void UpdateTheme(string themeName)
        {
            string themeFile = themeName.Contains("Dark") ? "DarkTheme.xaml" : "LightTheme.xaml";
            try
            {
                _currentThemeResource = new ResourceDictionary
                {
                    Source = new Uri($"pack://application:,,,/AddinVeMong;component/Resources/Styles/{themeFile}", UriKind.Absolute)
                };
            }
            catch (Exception ex)
            {
                // Đề phòng lỗi đường dẫn thì nạp tạm LightTheme mặc định tránh sập ứng dụng
                _currentThemeResource = new ResourceDictionary
                {
                    Source = new Uri("/AddinVeMong;component/Resources/Styles/LightTheme.xaml", UriKind.RelativeOrAbsolute)
                };
            }
        }
    }
}