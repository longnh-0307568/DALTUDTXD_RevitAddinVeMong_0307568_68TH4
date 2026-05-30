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

            DisplayText = $"PROJECT: ADD-IN THIẾT KẾ MÓNG ĐƠN VÁT\n";
        }
    }
}