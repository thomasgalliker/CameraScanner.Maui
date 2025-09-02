using CommunityToolkit.Maui;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Extensions;
using CommunityToolkit.Maui.Views;
using Microsoft.Extensions.Logging;

namespace CameraDemoApp.Services.Navigation
{
    public class PopupService : IPopupService2
    {
        private readonly ILogger logger;
        private readonly IPageResolver pageResolver;
        private readonly INavigationService navigationService;
        private readonly IPopupService popupService;

        public PopupService(
            ILogger<PopupService> logger,
            IPageResolver pageResolver,
            INavigationService navigationService,
            IPopupService popupService)
        {
            this.logger = logger;
            this.pageResolver = pageResolver;
            this.navigationService = navigationService;
            this.popupService = popupService;
        }

        public Task<TResult> ShowPopupAsync<TResult>(string page)
        {
            return this.ShowPopupAsync<TResult>(page, null);
        }

        public Task<TResult> ShowPopupAsync<TResult>(string page, object parameter)
        {
            return this.ShowPopupAsync<object, TResult>(page, parameter);
        }

        public async Task<TResult> ShowPopupAsync<TParameter, TResult>(string pageName, TParameter parameter)
        {
            try
            {
                var bindableObject = this.pageResolver.ResolvePage<BindableObject>(pageName);

                if (parameter == null)
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo>(bindableObject, p => p.NavigatedToAsync());
                }
                else
                {
                    await PageUtilities.InvokeViewAndViewModelActionAsync<INavigatedTo<TParameter>>(bindableObject, p => p.NavigatedToAsync(parameter));
                }

                var navigation = this.navigationService.Navigation;

                IPopupResult<TResult> popupResult;
                if (bindableObject is Popup popup)
                {
                    popupResult = await navigation.ShowPopupAsync<TResult>(popup);
                }
                else if (bindableObject is ContentPage contentPage)
                {
                    popupResult = await navigation.ShowPopupAsync<TResult>(contentPage.Content);
                }
                else
                {
                    throw new InvalidOperationException($"Resolved '{pageName}' must be of type {nameof(ContentPage)} or {nameof(Popup)}");
                }

                if (popupResult is IPopupResult<TResult> result)
                {
                    return result.Result;
                }

                return default;
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ShowPopupAsync failed with exception");
                throw;
            }
        }

        public Task ClosePopupAsync()
        {
            return this.ClosePopupAsync<object>(null);
        }

        public async Task ClosePopupAsync<TResult>(TResult result)
        {
            try
            {
                var navigation = this.navigationService.Navigation;

                if (result == null)
                {
                    await this.popupService.ClosePopupAsync(navigation);
                }
                else
                {
                    await this.popupService.ClosePopupAsync(navigation, result);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, "ClosePopupAsync failed with exception");
                throw;
            }
        }
    }
}