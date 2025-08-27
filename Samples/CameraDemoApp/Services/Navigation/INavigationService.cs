namespace CameraDemoApp.Services.Navigation
{
    public interface INavigationService
    {
        /// <summary>
        /// Pushes the given <paramref name="page"/> to the navigation stack.
        /// </summary>
        Task PushAsync(string page);

        /// <summary>
        /// Pushes the given <paramref name="page"/> with parameter <paramref name="parameter"/> to the navigation stack.
        /// </summary>
        Task PushAsync<T>(string page, T parameter);

        /// <summary>
        /// Pushes the given <paramref name="page"/> to the navigation stack in a modal context.
        /// </summary>
        Task PushModalAsync(string page);

        /// <summary>
        /// Pushes the given <paramref name="page"/> with parameter <paramref name="parameter"/> to the navigation stack in a modal context.
        /// </summary>
        Task PushModalAsync<T>(string page, T parameter);

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

        INavigation Navigation { get; }
    }
}