namespace CameraDemoApp.Services.Navigation
{
    public interface INavigationService
    {
        /// <summary>
        /// Pushes the given <paramref name="page"/> to the navigation stack.
        /// </summary>
        Task PushAsync(string page);

        /// <summary>
        /// Pushes the given <paramref name="page"/> to the navigation stack in a modal context.
        /// </summary>
        Task PushModalAsync(string page);

        /// <summary>
        /// Pops back from the current page.
        /// </summary>
        Task PopAsync();

        /// <summary>
        /// Pops back to the root page of the navigation stack.
        /// </summary>
        Task PopToRootAsync();

        /// <summary>
        /// Pops a modal page from the navigation stack.
        /// </summary>
        Task PopModalAsync();
    }
}