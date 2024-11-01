using CameraDemoApp.Extensions;
using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics.Platform;

namespace CameraDemoApp.ViewModels
{
    public class CameraPreviewViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IDialogService dialogService;

        private IRelayCommand toggleCameraFacingCommand;
        private CameraFacing cameraFacing;
        private float? requestZoomFactor;
        private IRelayCommand zoomInCommand;
        private IRelayCommand zoomOutCommand;
        private float? currentZoomFactor;
        private bool torchOn;
        private IRelayCommand toggleTorchCommand;
        private IAsyncRelayCommand shutterCommand;
        private IRelayCommand imageCapturedCommand;
        private bool captureNextFrame;
        private float? minZoomFactor;
        private float? maxZoomFactor;
        private IAsyncRelayCommand closeCommand;
        private Func<Task> shutterAction;

        public CameraPreviewViewModel(
            ILogger<CameraPreviewViewModel> logger,
            INavigationService navigationService,
            IDialogService dialogService)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.dialogService = dialogService;

            this.CameraFacing = CameraFacing.Front;
        }

        public CameraFacing CameraFacing
        {
            get => this.cameraFacing;
            private set
            {
                if (this.SetProperty(ref this.cameraFacing, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public IRelayCommand ToggleCameraFacingCommand
        {
            get => this.toggleCameraFacingCommand ??= new RelayCommand(this.ToggleCameraFacing);
        }

        private void ToggleCameraFacing()
        {
            this.CameraFacing = this.CameraFacing.Next();
        }

        public float? RequestZoomFactor
        {
            get => this.requestZoomFactor;
            private set
            {
                if (this.SetProperty(ref this.requestZoomFactor, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public float? CurrentZoomFactor
        {
            get => this.currentZoomFactor;
            set
            {
                if (this.SetProperty(ref this.currentZoomFactor, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public float? MinZoomFactor
        {
            get => this.minZoomFactor;
            set
            {
                if (this.SetProperty(ref this.minZoomFactor, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public float? MaxZoomFactor
        {
            get => this.maxZoomFactor;
            set
            {
                if (this.SetProperty(ref this.maxZoomFactor, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public IRelayCommand ZoomInCommand
        {
            get => this.zoomInCommand ??= new RelayCommand(this.ZoomIn);
        }

        private void ZoomIn()
        {
            this.RequestZoomFactor = this.CurrentZoomFactor + 0.1F;
        }

        public IRelayCommand ZoomOutCommand
        {
            get => this.zoomOutCommand ??= new RelayCommand(this.ZoomOut);
        }

        private void ZoomOut()
        {
            this.RequestZoomFactor = this.CurrentZoomFactor - 0.1F;
        }

        public IAsyncRelayCommand ShutterCommand
        {
            get => this.shutterCommand ??= new AsyncRelayCommand(this.ShutterAsync);
        }

        public Func<Task> ShutterAction
        {
            set => this.shutterAction = value;
            get => this.shutterAction;
        }

        private async Task ShutterAsync()
        {
            try
            {
                await this.ShutterAction?.Invoke();
            }
            catch (Exception _)
            {
              //  await this.dialogService.DisplayAlertAsync(
              //"ImageCaptured",
              //"Successfully returned PlatformImage",
              //"OK");
            }
        }

        public IRelayCommand ImageCapturedCommand
        {
            get => this.imageCapturedCommand ??= new RelayCommand<PlatformImage>(this.ImageCaptured);
        }

        private void ImageCaptured(PlatformImage image)
        {
            _ = this.dialogService.DisplayAlertAsync(
                "ImageCaptured",
                "Successfully returned PlatformImage",
                "OK");
        }

        public bool TorchOn
        {
            get => this.torchOn;
            set
            {
                if (this.SetProperty(ref this.torchOn, value))
                {
                    this.OnPropertyChanged(nameof(this.DebugInfo));
                }
            }
        }

        public IRelayCommand ToggleTorchCommand
        {
            get => this.toggleTorchCommand ??= new RelayCommand(this.ToggleTorch);
        }

        private void ToggleTorch()
        {
            this.TorchOn = !this.TorchOn;
        }

        public IAsyncRelayCommand CloseCommand
        {
            get => this.closeCommand ??= new AsyncRelayCommand(this.CloseAsync);
        }

        private async Task CloseAsync()
        {
            await this.navigationService.PopModalAsync();
        }

        public string DebugInfo
        {
            get
            {
                return
                    $"TorchOn: {this.TorchOn}{Environment.NewLine}" +
                    $"RequestZoomFactor: {this.RequestZoomFactor?.ToString() ?? "null"}{Environment.NewLine}" +
                    $"CurrentZoomFactor: {this.CurrentZoomFactor?.ToString() ?? "null"}{Environment.NewLine}" +
                    $"MinZoomFactor: {this.MinZoomFactor}{Environment.NewLine}" +
                    $"MaxZoomFactor: {this.MaxZoomFactor}{Environment.NewLine}" +
                    $"CameraFacing: {this.CameraFacing}";
            }
        }
    }
}