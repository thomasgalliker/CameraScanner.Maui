using System.Reflection;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.Services.Navigation
{
    public class MauiNavigationService : INavigationService
    {
        private readonly ILogger logger;
        private readonly IServiceProvider serviceProvider;

        public MauiNavigationService(
            ILogger<MauiNavigationService> logger,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.serviceProvider = serviceProvider;
        }

        public async Task PushAsync(string pageName)
        {
            try
            {
                var page = this.ResolvePage(pageName);
                var navigation = GetNavigation();
                await navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PushAsync failed with exception");
                throw;
            }
        }

        public async Task PushModalAsync(string pageName)
        {
            try
            {
                var page = this.ResolvePage(pageName);
                var navigation = GetNavigation();
                await navigation.PushModalAsync(new NavigationPage(page));
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PushModalAsync failed with exception");
                throw;
            }
        }

        private Page ResolvePage(string pageName)
        {
            var pageTypes = FindTypesWithName(pageName);
            if (pageTypes.Length == 0)
            {
                throw new PageNavigationException($"Page with name '{pageName}' not found");
            }

            if (pageTypes.Length > 1)
            {
                throw new PageNavigationException(
                    $"Multiple pages found for name '{pageName}': " +
                    $"{string.Join($"> {Environment.NewLine}", pageTypes.Select(t => t.FullName))}");
            }

            var pageType = pageTypes.Single();
            var page = (Page)this.serviceProvider.GetRequiredService(pageType);

            var viewModelName = pageName.Substring(0, pageName.LastIndexOf("Page")) + "ViewModel";
            var viewModelTypes = FindTypesWithName(viewModelName);

            if (viewModelTypes.Length == 0)
            {
                this.logger.LogInformation($"View model with name '{viewModelName}' not found");
            }
            else if (viewModelTypes.Length == 1)
            {
                var viewModelType = viewModelTypes.Single();
                var viewModel = this.serviceProvider.GetRequiredService(viewModelType);
                page.BindingContext = viewModel;
            }
            else
            {
                throw new PageNavigationException(
                    $"Multiple view models found for name '{viewModelName}': " +
                    $"{string.Join($"> {Environment.NewLine}", viewModelTypes.Select(t => t.FullName))}");
            }

            return page;
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

        private static Type[] FindTypesWithName(string typeName)
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => string.Equals(t.Name, typeName, StringComparison.InvariantCultureIgnoreCase))
                .ToArray();
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
    }

    public class PageNavigationException : Exception
    {
        public PageNavigationException(string message) : base(message)
        {
        }
    }
}