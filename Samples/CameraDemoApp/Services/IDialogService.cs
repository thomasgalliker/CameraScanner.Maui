namespace CameraDemoApp.Services
{
    public interface IDialogService
    {
        Task DisplayAlertAsync(string title, string message, string accept);

        Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel);
    }
}