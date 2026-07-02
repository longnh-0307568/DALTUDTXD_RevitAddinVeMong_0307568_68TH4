using System.Windows;
using AddinVeMong.ViewModels;

namespace AddinVeMong.Views
{
    public partial class AboutView : Window
    {
        public AboutView()
        {
            InitializeComponent();

            // Kết nối View với ViewModel bằng cách gán DataContext tại đây
            this.DataContext = new AboutViewModel();
        }
    }
}