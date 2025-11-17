using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Graphics.Platform;
using Superdev.Maui.Navigation;

namespace CameraDemoApp.ViewModels
{
    public class ImageViewerViewModel : ObservableObject, INavigatedTo<PlatformImage>
    {
        private readonly ILogger logger;
        private readonly INavigationService navigationService;
        private readonly IShare share;

        private PlatformImage platformImage;
        private ImageSource image;
        private IAsyncRelayCommand closeCommand;
        private IAsyncRelayCommand shareCommand;

        public ImageViewerViewModel(
            ILogger<ImageViewerViewModel> logger,
            INavigationService navigationService,
            IShare share)
        {
            this.logger = logger;
            this.navigationService = navigationService;
            this.share = share;
        }

        public Task NavigatedToAsync(PlatformImage platformImage)
        {
            this.platformImage = platformImage;
            this.Image = ImageSource.FromStream(() => platformImage.AsStream());
            return Task.CompletedTask;
        }

        public ImageSource Image
        {
            get => this.image;
            private set => this.SetProperty(ref this.image, value);
        }

        public IAsyncRelayCommand ShareCommand
        {
            get => this.shareCommand ??= new AsyncRelayCommand(this.ShareAsync);
        }

        private async Task ShareAsync()
        {
            {
                try
                {
                    var fullPath = Path.Combine(FileSystem.Current.CacheDirectory, $"{Guid.NewGuid():D}.png");
                    var imageBytes = await this.platformImage.AsBytesAsync();
                    await File.WriteAllBytesAsync(fullPath, imageBytes);

                    var shareRequest = new ShareFileRequest
                    {
                        Title = $"Image Sharing",
                        File = new ShareFile(fullPath)
                    };
                    await this.share.RequestAsync(shareRequest);
                }
                catch (Exception ex)
                {
                    this.logger.LogError(ex, "ShareAsync failed with exception");
                }
            }
        }

        public IAsyncRelayCommand CloseCommand
        {
            get => this.closeCommand ??= new AsyncRelayCommand(this.CloseAsync);
        }

        private async Task CloseAsync()
        {
            await this.navigationService.PopModalAsync();
        }
    }
}