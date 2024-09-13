using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class FilePickerViewModel : ObservableObject
    {
        private readonly ILogger logger;
        private readonly IMediaPicker mediaPicker;
        private readonly IBarcodeScanner barcodeScanner;

        private ImageSource image;
        private IAsyncRelayCommand pickPhotoCommand;
        private BarcodeResult[] barcodeResults;

        public FilePickerViewModel(
            ILogger<FilePickerViewModel> logger,
            IMediaPicker mediaPicker,
            IBarcodeScanner barcodeScanner)
        {
            this.logger = logger;
            this.mediaPicker = mediaPicker;
            this.barcodeScanner = barcodeScanner;
        }

        public ImageSource Image
        {
            get => this.image;
            private set => this.SetProperty(ref this.image, value);
        }

        public BarcodeResult[] BarcodeResults
        {
            get => this.barcodeResults;
            private set => this.SetProperty(ref this.barcodeResults, value);
        }

        public IAsyncRelayCommand PickPhotoCommand
        {
            get => this.pickPhotoCommand ??= new AsyncRelayCommand(this.PickPhotoAsync);
        }

        private async Task PickPhotoAsync()
        {
            try
            {
                var options = new MediaPickerOptions { Title = "Test picker" };
                var fileResult = await this.mediaPicker.PickPhotoAsync(options);
                if (fileResult != null)
                {
                    // save the file into local storage
                    //var localFilePath = Path.Combine(FileSystem.CacheDirectory, fileResult.FileName);

                    //using var sourceStream = await fileResult.OpenReadAsync();
                    //using var localFileStream = File.OpenWrite(localFilePath);

                    //await sourceStream.CopyToAsync(localFileStream);

                    var barcodeResults = await this.barcodeScanner.ScanFromImageAsync(fileResult);
                    this.Image = ImageSource.FromFile(fileResult.FullPath);
                    this.BarcodeResults = barcodeResults.ToArray();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PickPhotoAsync failed with exception");
                this.BarcodeResults = [];
                this.Image = null;
            }
        }
    }
}
