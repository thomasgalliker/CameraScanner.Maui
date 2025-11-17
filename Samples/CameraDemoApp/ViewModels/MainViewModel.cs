using System.Windows.Input;
using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Superdev.Maui.Navigation;
using ResourceLoader = System.Reflection.ResourceLoader;

namespace CameraDemoApp.ViewModels
{
    public class MainViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;
        private readonly ICameraPermissions cameraPermissions;
        private readonly ILauncher launcher;
        private readonly IAudioService audioService;

        private IAsyncRelayCommand appearingCommand;
        private bool isInitialized;
        private bool authorizationStatus;
        private IAsyncRelayCommand checkCameraPermissionsCommand;
        private IAsyncRelayCommand requestCameraPermissionsCommand;
        private IAsyncRelayCommand checkAndRequestCameraPermissionsCommand;
        private IAsyncRelayCommand<string> navigateToPageCommand;
        private IAsyncRelayCommand<string> navigateToModalPageCommand;
        private IAsyncRelayCommand<string> openUrlCommand;
        private IRelayCommand playSoundCommand;
        private IRelayCommand stopSoundCommand;

        public MainViewModel(
            ILogger<MainViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService,
            ICameraPermissions cameraPermissions,
            ILauncher launcher,
            IAudioService audioService)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;
            this.cameraPermissions = cameraPermissions;
            this.launcher = launcher;
            this.audioService = audioService;
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
                await this.CheckCameraPermissionsAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "InitializeAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", "Initialization failed", "OK");
            }
        }

        public bool AuthorizationStatus
        {
            get => this.authorizationStatus;
            private set => this.SetProperty(ref this.authorizationStatus, value);
        }

        public ICommand CheckCameraPermissionsCommand
        {
            get => this.checkCameraPermissionsCommand ??= new AsyncRelayCommand(this.CheckCameraPermissionsAsync);
        }

        private async Task CheckCameraPermissionsAsync()
        {
            try
            {
                this.AuthorizationStatus = await this.cameraPermissions.CheckPermissionAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "CheckCameraPermissionsAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"Check for camera permissions failed: {ex.Message}", "OK");
            }
        }

        public ICommand RequestCameraPermissionsCommand
        {
            get => this.requestCameraPermissionsCommand ??= new AsyncRelayCommand(this.RequestCameraPermissionsAsync);
        }

        private async Task RequestCameraPermissionsAsync()
        {
            try
            {
                this.AuthorizationStatus = await this.cameraPermissions.RequestPermissionAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "RequestCameraPermissionsAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"Request for camera permissions failed: {ex.Message}", "OK");
            }
        }


        public ICommand CheckAndRequestCameraPermissionsCommand
        {
            get => this.checkAndRequestCameraPermissionsCommand ??= new AsyncRelayCommand(this.CheckAndRequestCameraPermissionsAsync);
        }

        private async Task CheckAndRequestCameraPermissionsAsync()
        {
            try
            {
                this.AuthorizationStatus = await this.cameraPermissions.CheckAndRequestPermissionAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "CheckAndRequestCameraPermissionsAsync failed with exception");
                await this.dialogService.DisplayAlertAsync("Error", $"CheckAndRequest for camera permissions failed: {ex.Message}", "OK");
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

        public IAsyncRelayCommand<string> NavigateToModalPageCommand
        {
            get => this.navigateToModalPageCommand ??= new AsyncRelayCommand<string>(this.NavigateToModalPageAsync);
        }

        private async Task NavigateToModalPageAsync(string page)
        {
            await this.navigationService.PushModalAsync(page);
        }

        public IAsyncRelayCommand<string> OpenUrlCommand
        {
            get => this.openUrlCommand ??= new AsyncRelayCommand<string>(this.OpenUrlAsync);
        }

        private async Task OpenUrlAsync(string url)
        {
            try
            {
                await this.launcher.TryOpenAsync(url);
            }
            catch
            {
                // Ignore exceptions
            }
        }

        public IRelayCommand PlaySoundCommand
        {
            get => this.playSoundCommand ??= new RelayCommand(this.PlaySoundAsync);
        }

        private void PlaySoundAsync()
        {
            var assembly = typeof(App).Assembly;
            var beepStream = ResourceLoader.Current.GetEmbeddedResourceStream(assembly, "beep-90395.mp3");
            this.audioService.SetSource(beepStream);
            this.audioService.Volume = 1f;
            this.audioService.Play();
        }

        public IRelayCommand StopSoundCommand
        {
            get => this.stopSoundCommand ??= new RelayCommand(this.StopSoundAsync);
        }

        private void StopSoundAsync()
        {
            this.audioService.Stop();
        }
    }
}