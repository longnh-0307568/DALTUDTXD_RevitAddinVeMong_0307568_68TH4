using Autodesk.Revit.UI;
using System.Windows;

namespace AddinVeMong.Views
{
    public partial class MainWindowView : Window
    {
        public MainWindowView(ExternalCommandData _commandData)
        {
            InitializeComponent();

            // Initialize ViewModel and assign to DataContext
            this.DataContext = new AddinVeMong.ViewModels.MainViewModel(_commandData);
        }
    }
}