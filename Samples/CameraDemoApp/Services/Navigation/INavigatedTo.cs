namespace CameraDemoApp.Services.Navigation
{
    public interface INavigatedTo<in T>
    {
        Task NavigatedToAsync(T barcodeResult);
    }
}