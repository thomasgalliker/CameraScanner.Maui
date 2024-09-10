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

                var pagesIsUsed = CheckIfPageIsUsed(targetPage);
                if (pagesIsUsed)
                {
                    return;
                }

                // If the target page is no longer used anywhere,
                // we can safely unsubscribe from Unloaded event and call DisconnectHandler.
                targetPage.Unloaded -= OnPageUnloaded;

                if (view.Handler is not IElementHandler elementHandler)
                {
                    Debug.WriteLine(
                        $"HandlerCleanUpHelper.OnPageUnloaded: Page \"{targetPage.GetType().Name}\" is no longer present on the navigation stack " +
                        $"--> {view.GetType().Name}.Handler is null");
                }
                else
                {
                    Debug.WriteLine(
                        $"HandlerCleanUpHelper.OnPageUnloaded: Page \"{targetPage.GetType().Name}\" is no longer present on the navigation stack " +
                        $"--> {elementHandler.GetType().Name}.DisconnectHandler()");
                    elementHandler.DisconnectHandler();
                }
            }

            Debug.WriteLine($"HandlerCleanUpHelper.AddCleanUpEvent for Page \"{targetPage.GetType().Name}\"");
            targetPage.Unloaded += OnPageUnloaded;
        }

        private static bool CheckIfPageIsUsed(Page targetPage)
        {
            // For apps with shell navigation, we check if the target page
            // is still used in Shell.Current or one of its children.
            if (Shell.Current is Shell shell)
            {
                var pages = GetActivePages(shell);
                var pageExists = pages.Any(p => p == targetPage);
                return pageExists;
            }

            // For apps with classic navigation, we check the target page
            // is part of the NavigationStack or the ModalStack.
            var navigation = targetPage.Navigation;
            if (navigation != null &&
                (navigation.NavigationStack.Any(p => p == targetPage) ||
                 navigation.ModalStack.Any(p => p == targetPage)))
            {
                return true;
            }

            return false;
        }

        private static IEnumerable<Page> GetActivePages(Shell shell)
        {
            var hashSet = new HashSet<Page>();

            foreach (var shellItem in shell.Items)
            {
                foreach (var page in WalkToPage(shellItem))
                {
                    hashSet.Add(page);
                }

                foreach (var shellSection in shellItem.Items)
                {
                    foreach (var page in WalkToPage(shellSection))
                    {
                        hashSet.Add(page);
                    }
                }
            }

            return hashSet;
        }

        private static IEnumerable<Page> WalkToPage(Element element)
        {
            switch (element)
            {
                case Shell shell:
                    return WalkToPage(shell.CurrentItem);

                case ShellItem shellItem:
                    return WalkToPage(shellItem.CurrentItem);

                case ShellSection shellSection:
                    IShellSectionController controller = shellSection;
                    var children = controller.GetItems().OfType<IShellContentController>();
                    return children.Select(c => c.Page);
            }

            return [];
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