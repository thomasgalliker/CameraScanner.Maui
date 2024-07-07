using CameraDemoApp.ViewModels;
using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Input;

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