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

        private void OnCentricInput(object sender, RoutedEventArgs e)
        {
            InputCentric.Visibility = Visibility.Visible;
            InputEccentric.Visibility = Visibility.Collapsed;
        }

        private void OnEccentricInput(object sender, RoutedEventArgs e)
        {
            InputCentric.Visibility = Visibility.Collapsed;
            InputEccentric.Visibility = Visibility.Visible;
        }
    }
}
