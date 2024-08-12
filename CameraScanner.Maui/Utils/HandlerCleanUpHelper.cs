using System.Diagnostics;

namespace CameraScanner.Maui.Utils
{
    /// <summary>
    /// This helper class is used because MAUI does not call DisconnectHandler automatically.
    /// </summary>
    internal static class HandlerCleanUpHelper
    {
        public static void AddCleanUpEvent(this IView view)
        {
            if (view is not Element element)
            {
                return;
            }

            var parentPage = element.GetRealParentPages().FirstOrDefault();
            var targetPage = GetTarget(parentPage);

            async void OnPageUnloaded(object sender, EventArgs e)
            {
                await Task.Delay(200);

                var navigation = GetNavigation(targetPage);
                if (navigation != null &&
                    (navigation.NavigationStack.Any(p => p == targetPage) ||
                     navigation.ModalStack.Any(p => p == targetPage)))
                {
                    return;
                }

                targetPage.Unloaded -= OnPageUnloaded;

                var elementHandler = view.Handler as IElementHandler;
                Debug.WriteLine($"HandlerCleanUpHelper.OnPageUnloaded: Page \"{targetPage.GetType().Name}\" is no longer present on the navigation stack " +
                                $"--> {(elementHandler != null ? $"{elementHandler.GetType().Name}." : "")}DisconnectHandler()");
                elementHandler?.DisconnectHandler();
            }

            Debug.WriteLine($"HandlerCleanUpHelper.AddCleanUpEvent for Page \"{targetPage.GetType().Name}\"");
            targetPage.Unloaded += OnPageUnloaded;
        }

        private static INavigation GetNavigation(Page page)
        {
            if (Shell.Current?.Navigation is INavigation navigation)
            {
                return navigation;
            }

            if (page != null)
            {
                return page.Navigation;
            }

            throw new Exception("HandlerCleanUpHelper.GetNavigation could not find INavigation");
        }

        private static IEnumerable<Page> GetRealParentPages(this Element element)
        {
            var current = element;

            while (!IsApplicationOrNull(current.RealParent))
            {
                current = current.RealParent;

                if (current is Page page)
                {
                    yield return page;
                }
            }
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

        private static bool IsApplicationOrNull(object element)
        {
            return element == null || element is IApplication;
        }
    }
}