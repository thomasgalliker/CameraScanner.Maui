using System.Reflection;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.Services
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

                await Application.Current.MainPage.Navigation.PushAsync(page);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PushAsync failed with exception");
                throw;
            }
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
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "PopAsync failed with exception");
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