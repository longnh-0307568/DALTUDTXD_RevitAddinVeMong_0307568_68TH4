using Autodesk.Revit.UI;
using System.Windows;


namespace AddinVeMong.Views
{
    /// <summary>
    /// Interaction logic for MainWindowView.xaml
    /// </summary>
    public partial class MainWindowView : Window
    {
        private ExternalCommandData _commanData;
        public MainWindowView(ExternalCommandData _commandData)
        {
            InitializeComponent();

            _commanData = _commandData;
        }

        private void BtnDungTam_Click(object sender, RoutedEventArgs e)
        {
            InputDungTam.Visibility = Visibility.Visible;
            InputLechTam.Visibility = Visibility.Collapsed;
        }

        private void BtnLechTam_Click(object sender, RoutedEventArgs e)
        {
            InputDungTam.Visibility = Visibility.Collapsed;
            InputLechTam.Visibility = Visibility.Visible;
        }
    }
}
