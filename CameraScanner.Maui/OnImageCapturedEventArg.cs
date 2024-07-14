using Microsoft.Maui.Graphics.Platform;

namespace CameraScanner.Maui
{
    public class OnImageCapturedEventArg : EventArgs
    {
        public PlatformImage Image { get; set; } = null;
    }
}