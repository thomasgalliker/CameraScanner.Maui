using CameraDemoApp.Services.Navigation;
using CameraScanner.Maui;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.ViewModels
{
    public class FilePickerViewModel : ObservableObject, INavigatedTo
    {
        private readonly ILogger logger;
        private readonly ILoggerFactory loggerFactory;
        private readonly IMediaPicker mediaPicker;
        private readonly IDialogService dialogService;
        private readonly INavigationService navigationService;
        private readonly IBarcodeScanner barcodeScanner;

        private ImageSource image;
        private IAsyncRelayCommand pickPhotoCommand;
        private BarcodeResultItemViewModel[] barcodeResults;
        private IAsyncRelayCommand<BarcodeResult> barcodeResultTappedCommand;

        public FilePickerViewModel(
            ILogger<FilePickerViewModel> logger,
            ILoggerFactory loggerFactory,
            IMediaPicker mediaPicker,
            IDialogService dialogService,
            INavigationService navigationService,
            IBarcodeScanner barcodeScanner)
        {
            this.logger = logger;
            this.loggerFactory = loggerFactory;
            this.mediaPicker = mediaPicker;
            this.dialogService = dialogService;
            this.navigationService = navigationService;
            this.barcodeScanner = barcodeScanner;
        }

        public Task NavigatedToAsync()
        {
            _ = this.PickPhotoAsync();
            return Task.CompletedTask;
        }

        public ImageSource Image
        {
            get => this.image;
            private set => this.SetProperty(ref this.image, value);
        }

        public BarcodeResultItemViewModel[] BarcodeResults
        {
            get => this.barcodeResults;
            private set
            {
                if (this.SetProperty(ref this.barcodeResults, value))
                {
                    this.OnPropertyChanged(nameof(this.BarcodeResultsCountText));
                }
            }
        }

        public string BarcodeResultsCountText
        {
            get
            {
                if (this.BarcodeResults is not BarcodeResultItemViewModel[] barcodeResults)
                {
                    return null;
                }

                return $"Number of barcodes found: {barcodeResults.Length}";
            }
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
                    this.Image = ImageSource.FromFile(fileResult.FullPath);

                    var barcodeResultItemViewModelLogger = this.loggerFactory.CreateLogger<BarcodeResultItemViewModel>();
                    var barcodeResults = await this.barcodeScanner.ScanFromImageAsync(fileResult);
                    this.BarcodeResults = barcodeResults
                        .Select(r => new BarcodeResultItemViewModel(barcodeResultItemViewModelLogger, this.navigationService, r))
                        .ToArray();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PickPhotoAsync failed with exception");
                this.BarcodeResults = [];
                this.Image = null;
            }
        }

        public IAsyncRelayCommand<BarcodeResult> BarcodeResultTappedCommand
        {
            get => this.barcodeResultTappedCommand ??= new AsyncRelayCommand<BarcodeResult>(this.OnBarcodeResultTapped);
        }

        private async Task OnBarcodeResultTapped(BarcodeResult barcodeResult)
        {
            try
            {
                await this.navigationService.PushAsync("BarcodeResultDetailPage", barcodeResult);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "OnBarcodeResultTapped failed with exception");
            }
        }
    }
}
