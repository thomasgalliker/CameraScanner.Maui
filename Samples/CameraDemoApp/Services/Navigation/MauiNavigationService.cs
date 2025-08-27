using Microsoft.Extensions.Logging;

namespace CameraDemoApp.Services.Navigation
{
    public class MauiNavigationService : INavigationService
    {
        private readonly ILogger logger;
        private readonly IPageResolver pageResolver;

        public MauiNavigationService(
            ILogger<MauiNavigationService> logger,
            IPageResolver pageResolver)
        {
            this.logger = logger;
            this.pageResolver = pageResolver;
        }

        public Task PushAsync(string pageName)
        {
            return this.PushAsync<object>(pageName, null);
        }

        public async Task PushAsync<T>(string pageName, T parameter)
        {
            try
            {
                var page = this.pageResolver.ResolvePage(pageName);
                var navigation = GetNavigation();
                await navigation.PushAsync(page);

                if (parameter == null)
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo>(page, p => p.NavigatedToAsync());
                }
                else
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo<T>>(page, p => p.NavigatedToAsync(parameter));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PushAsync failed with exception");
                throw;
            }
        }

        public Task PushModalAsync(string pageName)
        {
            return this.PushModalAsync<object>(pageName, null);
        }

        public async Task PushModalAsync<T>(string pageName, T parameter)
        {
            try
            {
                var page = this.pageResolver.ResolvePage(pageName);
                var navigation = GetNavigation();
                await navigation.PushModalAsync(new NavigationPage(page));

                if (parameter == null)
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo>(page, p => p.NavigatedToAsync());
                }
                else
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo<T>>(page, p => p.NavigatedToAsync(parameter));
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PushModalAsync failed with exception");
                throw;
            }
        }

        private static INavigation GetNavigation()
        {
            if (Shell.Current != null)
            {
                throw new NotSupportedException(
                    $"{nameof(MauiNavigationService)} does currently not support AppShell navigation");
            }

            if (Application.Current?.MainPage is not Page page)
            {
                throw new PageNavigationException("Application.Current.MainPage is not set");
            }

            var targetPage = GetTarget(page);
            return targetPage.Navigation;
        }

        private static Page GetTarget(Page target)
        {
            return target switch
            {
                FlyoutPage flyout => GetTarget(flyout.Detail),
                TabbedPage tabbed => GetTarget(tabbed.CurrentPage),
                NavigationPage navigation => GetTarget(navigation.CurrentPage) ?? navigation,
                ContentPage page => page,
                null => null,
                _ => throw new NotSupportedException($"The page type '{target.GetType().FullName}' is not supported.")
            };
        }

        public async Task PopAsync()
        {
            try
            {
                var navigation = GetNavigation();
                await navigation.PopAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PopAsync failed with exception");
                throw;
            }
        }

        public async Task PopToRootAsync()
        {
            try
            {
                var navigation = GetNavigation();
                await navigation.PopToRootAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PopToRootAsync failed with exception");
                throw;
            }
        }

        public async Task PopModalAsync()
        {
            try
            {
                var navigation = GetNavigation();
                await navigation.PopModalAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PopModalAsync failed with exception");
                throw;
            }
        }

        public INavigation Navigation => Application.Current?.Windows[0].Page?.Navigation ?? throw new InvalidOperationException($"{nameof(Page.Navigation)} not found");
    }

    public class PageNavigationException : Exception
    {
        public PageNavigationException(string message) : base(message)
        {
        }
    }
}