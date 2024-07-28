using CoreGraphics;

namespace CameraScanner.Maui.Platforms.Extensions
{
    internal static class CGRectExtensions
    {
        internal static CGRect InvertY(this CGRect rect)
        {
            return new CGRect(rect.X, 1 - rect.Y - rect.Height, rect.Width, rect.Height);
        }
    }
}
