using System.ComponentModel;
using System.Diagnostics;

namespace CameraScanner.Maui.Utils
{
    /// <summary>
    /// This helper class is used because MAUI does not call DisconnectHandler automatically.
    /// </summary>
    internal static class HandlerCleanUpHelper
    {
        internal static void AddCleanUpEvent(this IView view)
        {
            if (view is not Element element)
            {
                return;
            }

            var parentPage = element.GetRealParentPages().FirstOrDefault();
            var targetPage = PageHelper.GetTarget(parentPage);

            async void OnDisappearing(object sender, EventArgs e)
            {
                // For some reason we cannot use NavigatedFrom. When NavigatedFrom is fired,
                // the navigated page is still on the navigation stack which seems pretty odd.
                // The only way we could check if the page is navigated away is
                // to wait some milliseconds after the Disappearing event. It's a hack but it works well.
                await Task.Delay(200);

                var pagesIsUsed = CheckIfPageIsUsed(targetPage);
                if (pagesIsUsed)
                {
                    return;
                }

                // If the target page is no longer used anywhere on the navigation stack nor on the modal stack,
                // we can safely unsubscribe from Disappearing event and call DisconnectHandler.
                targetPage.Disappearing -= OnDisappearing;

                if (view.Handler is not IElementHandler elementHandler)
                {
                    Trace.WriteLine(
                        $"HandlerCleanUpHelper.OnNavigatedFrom: Page \"{GetPageNameForLogging(targetPage)}\" is no longer present on the navigation stack " +
                        $"--> {view.GetType().Name}.Handler is null");
                }
                else
                {
                    Trace.WriteLine(
                        $"HandlerCleanUpHelper.OnNavigatedFrom: Page \"{GetPageNameForLogging(targetPage)}\" is no longer present on the navigation stack " +
                        $"--> {elementHandler.GetType().Name}.DisconnectHandler()");
                    elementHandler.DisconnectHandler();
                }
            }

            targetPage.Disappearing += OnDisappearing;

            Trace.WriteLine($"HandlerCleanUpHelper.AddCleanUpEvent for \"{view.GetType().Name}\" on page \"{GetPageNameForLogging(targetPage)}\"");
        }

        private static string GetPageNameForLogging(Page targetPage)
        {
            var pageName = targetPage.GetType().Name;

            if (targetPage is IDebugPage internalPage)
            {
                pageName += $"[{nameof(internalPage.DebugId)}={internalPage.DebugId}]";
            }

            return pageName;
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
            {
                var mainPage = Application.Current.MainPage;
                var navigation = mainPage.Navigation;
                var pages = PageHelper.GetNavigationTree(navigation, mainPage).ToArray();
                var pageExists = pages.Any(p => p == targetPage);
                return pageExists;
            }
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

        private static bool IsApplicationOrNull(object element)
        {
            return element == null || element is IApplication;
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IDebugPage
    {
        public string DebugId { get; }
    }
}