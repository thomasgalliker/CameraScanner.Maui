using System.Windows.Input;
using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly ICameraPermissions cameraPermissions;
        private readonly IBarcodeScanner barcodeScanner;
        private IAsyncRelayCommand appearingCommand;

        private bool isInitialized;
        private bool authorizationStatus;
        private IAsyncRelayCommand requestCameraPermissionsCommand;
        private IAsyncRelayCommand<string> navigateToPageCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            ICameraPermissions cameraPermissions,
            IBarcodeScanner barcodeScanner)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.cameraPermissions = cameraPermissions;
            this.barcodeScanner = barcodeScanner;
        }


        public IAsyncRelayCommand AppearingCommand
        {
            get => this.appearingCommand ??= new AsyncRelayCommand(this.OnAppearingAsync);
        }

        private async Task OnAppearingAsync()
        {
            if (!this.isInitialized)
            {
                await this.InitializeAsync();
                this.isInitialized = true;
            }
        }

        private async Task InitializeAsync()
        {
            try
            {
                await this.UpdateAuthorizationStatusAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", "Initialization failed", "OK");
            }
        }

        private async Task UpdateAuthorizationStatusAsync()
        {
            this.AuthorizationStatus = await this.cameraPermissions.CheckPermissionAsync();
        }

        public bool AuthorizationStatus
        {
            get => this.authorizationStatus;
            private set => this.SetProperty(ref this.authorizationStatus, value);
        }

        public ICommand RequestCameraPermissionsCommand
        {
            get => this.requestCameraPermissionsCommand ??= new AsyncRelayCommand(this.RequestCameraPermissionsAsync);
        }

        private async Task RequestCameraPermissionsAsync()
        {
            try
            {
                //await this.cameraPermissions.CheckAndRequesPermissionAsync();
                await this.cameraPermissions.RequestPermissionAsync();
                await this.UpdateAuthorizationStatusAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RequestCameraPermissionsAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"Request for camera permissions failed: {ex.Message}", "OK");
            }
        }

        public IAsyncRelayCommand<string> NavigateToPageCommand
        {
            get => this.navigateToPageCommand ??= new AsyncRelayCommand<string>(this.NavigateToPageAsync);
        }

        private async Task NavigateToPageAsync(string page)
        {
            await this.navigationService.PushAsync(page);
        }

    }
}
