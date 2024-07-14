using CameraDemoApp.ViewModels;
using CommunityToolkit.Maui.Views;

namespace CameraDemoApp.Views
{
    public partial class ScannerConfigPopup : Popup
    {
        public ScannerConfigPopup(ScannerConfigViewModel viewModel)
        {
            this.InitializeComponent();
            this.BindingContext = viewModel;
        }
    }
}