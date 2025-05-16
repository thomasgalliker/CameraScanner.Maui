namespace CameraScanner.Maui.Utils
{
    internal static class PageHelper
    {
        internal static Page GetTarget(Page target)
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

        internal static string PrintNavigationPath()
        {
            var mainPage = Application.Current.MainPage;
            var pages = GetNavigationTree(mainPage).ToArray();
            var navigationPath = PrintNavigationPath(pages);
            return navigationPath;
        }

        private static string PrintNavigationPath(IEnumerable<Page> pages)
        {
            return pages.Aggregate("", (current, page) => $"{current}/{page?.GetType().Name ?? ""}");
        }

        internal static IEnumerable<Page> GetNavigationTree(Page page, bool modal = false)
        {
            if (page == null)
            {
                yield break;
            }

            var navigation = page.Navigation;

            switch (page)
            {
                case FlyoutPage flyoutPage:
                    yield return flyoutPage;
                    foreach (var p in GetNavigationTree(flyoutPage.Flyout))
                    {
                        yield return p;
                    }

                    foreach (var p in GetNavigationTree(flyoutPage.Detail))
                    {
                        yield return p;
                    }

                    break;

                case TabbedPage tabbedPage:
                    yield return tabbedPage;
                    foreach (var tab in tabbedPage.Children)
                    {
                        foreach (var p in GetNavigationTree(tab))
                        {
                            yield return p;
                        }
                    }

                    break;

                case NavigationPage navigationPage:
                    yield return navigationPage;

                    foreach (var childPage in navigationPage.InternalChildren.OfType<Page>())
                    {
                        foreach (var p in GetNavigationTree(childPage))
                        {
                            yield return p;
                        }
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
                    foreach (var p in GetNavigationTree(modalPage, modal: true))
                    {
                        yield return p;
                    }
                }
            }
        }
    }
}