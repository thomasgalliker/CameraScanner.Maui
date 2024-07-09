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
            private set => this.SetProperty(ref this.cameraFacing, value);
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
            private set => this.SetProperty(ref this.requestZoomFactor, value);
        }

        public float? CurrentZoomFactor
        {
            get => this.currentZoomFactor;
            set => this.SetProperty(ref this.currentZoomFactor, value);
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
    }
}