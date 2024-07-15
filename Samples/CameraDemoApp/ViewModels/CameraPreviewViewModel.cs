using CameraDemoApp.Extensions;
using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class CameraPreviewViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly IDialogService dialogService;

        private IRelayCommand toggleCameraFacingCommand;
        private CameraFacing cameraFacing;
        private float? requestZoomFactor;
        private IRelayCommand zoomInCommand;
        private IRelayCommand zoomOutCommand;
        private float? currentZoomFactor;
        private bool torchOn;
        private IRelayCommand toggleTorchCommand;

        public CameraPreviewViewModel(
            ILogger<CameraPreviewViewModel> logger,
            IDialogService dialogService)
        {
            this.logger = logger;
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


        public string DebugInfo
        {
            get
            {
                return
                    $"TorchOn: {this.TorchOn}{Environment.NewLine}" +
                    $"RequestZoomFactor: {this.RequestZoomFactor?.ToString() ?? "null"}{Environment.NewLine}" +
                    $"CurrentZoomFactor: {this.CurrentZoomFactor?.ToString() ?? "null"}{Environment.NewLine}" +
                    $"CameraFacing: {this.CameraFacing}";
            }
        }
    }
}