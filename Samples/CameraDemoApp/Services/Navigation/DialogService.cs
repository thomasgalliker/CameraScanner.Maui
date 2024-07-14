namespace CameraDemoApp.Services.Navigation
{
    public class DialogService : IDialogService
    {
        public Task DisplayAlertAsync(string title, string message, string accept)
        {
            var currentPage = Application.Current.MainPage;
            return currentPage.DisplayAlert(title, message, accept);
        }

        public Task<bool> DisplayAlertAsync(string title, string message, string accept, string cancel)
        {
            var currentPage = Application.Current.MainPage;
            return currentPage.DisplayAlert(title, message, accept, cancel);
        }
    }
}