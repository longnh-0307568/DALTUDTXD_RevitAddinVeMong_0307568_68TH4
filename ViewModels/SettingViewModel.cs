using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using AddinVeMong.Commands;

namespace AddinVeMong.ViewModels
{
    public class SettingViewModel
    {
        public string TitleText { get; set; }

        // Biến lưu trạng thái ngôn ngữ (Mặc định ban đầu luôn là Tiếng Việt)
        public static string CurrentLanguageSettings { get; set; } = "Tiếng Việt";

        // Biến tĩnh lưu trạng thái Theme
        public static string CurrentThemeSettings { get; set; } = "Sáng (Light)";

        public List<string> Languages { get; set; } = new List<string> { "Tiếng Việt", "English" };
        public List<string> Themes { get; set; } = new List<string> { "Sáng (Light)", "Tối (Dark)" };

        public string SelectedLanguage { get; set; }
        public string SelectedTheme { get; set; }

        public ICommand SaveSettingsCommand { get; set; }

        public SettingViewModel()
        {
            TitleText = "Cài đặt hệ thống";

            // Lấy lại cấu hình đã lưu trong biến tĩnh thay vì dùng lệnh kiểm tra hệ điều hành Windows
            SelectedLanguage = CurrentLanguageSettings;
            SelectedTheme = CurrentThemeSettings;

            SaveSettingsCommand = new RelayCommand(ExecuteSaveSettings);
        }

        private void ExecuteSaveSettings(object? parameter)
        {
            // 1. Cập nhật các biến tĩnh lưu trạng thái
            CurrentLanguageSettings = SelectedLanguage;
            CurrentThemeSettings = SelectedTheme;

            // 2. Cập nhật ngôn ngữ
            string cultureCode = SelectedLanguage == "English" ? "en" : "vi";
            var culture = new CultureInfo(cultureCode);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;

            // 3. Đổi qua ThemeManager
            AddinVeMong.Helpers.ThemeManager.UpdateTheme(SelectedTheme);

            if (cultureCode == "en")
            {
                MessageBox.Show("Settings saved successfully!", "Notification", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Lưu cài đặt thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }

            if (parameter is Window currentWindow)
            {
                currentWindow.Close();
            }
        }
    }
}