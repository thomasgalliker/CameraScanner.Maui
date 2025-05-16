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

            switch (page)
            {
                case FlyoutPage flyoutPage:
                    yield return flyoutPage;
                    foreach (var childPage in GetChildPages(flyoutPage))
                    {
                        yield return childPage;
                    }
                    break;

                case TabbedPage tabbedPage:
                    yield return tabbedPage;
                    foreach (var childPage in GetChildPages(tabbedPage))
                    {
                        yield return childPage;
                    }
                    break;

                case NavigationPage navigationPage:
                    yield return navigationPage;
                    foreach (var childPage in GetChildPages(navigationPage))
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
                var navigation = page.Navigation;
                foreach (var modalPage in navigation.ModalStack)
                {
                    foreach (var p in GetNavigationTree(modalPage, modal: true))
                    {
                        yield return p;
                    }
                }
            }
        }

        private static IEnumerable<Page> GetChildPages(NavigationPage navigationPage)
        {
            var pages = navigationPage.InternalChildren.OfType<Page>().ToArray();
            foreach (var page in pages)
            {
                yield return page;

                if (page is FlyoutPage flyoutPage)
                {
                    foreach (var childPage in GetChildPages(flyoutPage))
                    {
                        yield return childPage;
                    }
                }
                else if (page is TabbedPage tabbedPage)
                {
                    foreach (var childPage in GetChildPages(tabbedPage))
                    {
                        yield return childPage;
                    }
                }
            }
        }

        private static IEnumerable<Page> GetChildPages(TabbedPage tabbedPage)
        {
            foreach (var c in tabbedPage.Children)
            {
                foreach (var p in GetNavigationTree(c))
                {
                    yield return p;
                }
            }
        }

        private static IEnumerable<Page> GetChildPages(FlyoutPage flyoutPage)
        {
            foreach (var p in GetNavigationTree(flyoutPage.Flyout))
            {
                yield return p;
            }

            foreach (var p in GetNavigationTree(flyoutPage.Detail))
            {
                yield return p;
            }
        }
    }
}