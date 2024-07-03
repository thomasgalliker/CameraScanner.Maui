namespace CameraDemoApp.Services
{
    public interface INavigationService
    {
        /// <summary>
        /// Pushes the given <paramref name="page"/> to the navigation stack.
        /// </summary>
        Task PushAsync(string page);

        /// <summary>
        /// Pops back from the current page.
        /// </summary>
        Task PopAsync();
    }
}