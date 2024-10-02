using System.ComponentModel;

namespace CameraScanner.Maui.Utils
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class PageHelper
    {
        public static Page GetTarget(Page target)
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

        public static string PrintNavigationPath()
        {
            var mainPage = Application.Current.MainPage;
            var navigation = mainPage.Navigation;
            var pages = GetNavigationTree(navigation, mainPage).ToArray();
            var navigationPath = PrintNavigationPath(pages);
            return navigationPath;
        }

        private static string PrintNavigationPath(IEnumerable<Page> pages)
        {
            return pages.Aggregate("", (current, page) => $"{current}/{(page?.GetType().Name ?? "")}");
        }

        public static IEnumerable<Page> GetNavigationTree(INavigation navigation, Page page, bool modal = false)
        {
            switch (page)
            {
                case FlyoutPage flyoutPage:
                    yield return flyoutPage;
                    foreach (var p in GetNavigationTree(flyoutPage.Flyout.Navigation, flyoutPage.Flyout))
                    {
                        yield return p;
                    }

                    foreach (var p in GetNavigationTree(flyoutPage.Detail.Navigation, flyoutPage.Detail))
                    {
                        yield return p;
                    }

                    break;

                case TabbedPage tabbedPage:
                    yield return tabbedPage;
                    foreach (var p in GetNavigationTree(tabbedPage.CurrentPage.Navigation, tabbedPage.CurrentPage))
                    {
                        yield return p;
                    }

                    break;

                case NavigationPage navigationPage:
                    yield return navigationPage;

                    foreach (var childPage in navigationPage.InternalChildren.OfType<Page>())
                    {
                        yield return childPage;
                    }
                    break;

                case ContentPage contentPage:
                    yield return contentPage;

                    break;
            }

            if (modal == false)
            {
                foreach (var modalPage in navigation.ModalStack)
                {
                    foreach (var p in GetNavigationTree(modalPage.Navigation, modalPage, modal: true))
                    {
                        yield return p;
                    }
                }
            }
        }
    }
}