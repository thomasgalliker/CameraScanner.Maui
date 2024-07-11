namespace CameraScanner.Maui.Utils
{
    internal static class HandlerCleanUpHelper
    {
        public static void AddCleanUpEvent(this View view)
        {
            if (view is not Element element)
            {
                return;
            }

            void OnPageUnloaded(object sender, EventArgs e)
            {
                foreach (var page in element.GetParentPages())
                {
                    page.Unloaded -= OnPageUnloaded;
                }

                view.Handler?.DisconnectHandler();
            }

            foreach (var page in element.GetParentPages())
            {
                page.Unloaded += OnPageUnloaded;
            }
        }

        private static IEnumerable<Page> GetParentPages(this Element element)
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

        public static bool IsApplicationOrNull(object element)
        {
            return element == null || element is IApplication;
        }
    }
}