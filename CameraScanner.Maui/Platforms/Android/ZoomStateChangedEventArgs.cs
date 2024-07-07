namespace CameraScanner.Maui.Platforms.Android
{
    internal class ZoomStateChangedEventArgs : EventArgs
    {
        internal ZoomStateChangedEventArgs(float zoomRatio, float minZoomRatio, float maxZoomRatio)
        {
            this.ZoomRatio = zoomRatio;
            this.MinZoomRatio = minZoomRatio;
            this.MaxZoomRatio = maxZoomRatio;
        }

        public float ZoomRatio { get; }

        public float MinZoomRatio { get; }

        public float MaxZoomRatio { get; }
    }
}