namespace CameraDemoApp.Services.Navigation
{
    public interface IPopupService2
    {
        Task<TResult> ShowPopupAsync<TResult>(string page);

        Task<TResult> ShowPopupAsync<TResult>(string page, object parameter);

        Task<TResult> ShowPopupAsync<TParameter, TResult>(string page, TParameter parameter);

        Task ClosePopupAsync();

        Task ClosePopupAsync<TResult>(TResult result);
    }
}