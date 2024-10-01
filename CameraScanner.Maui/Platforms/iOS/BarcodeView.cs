using AVFoundation;
using CoreAnimation;
using CoreGraphics;
using UIKit;

namespace CameraScanner.Maui
{
    [Preserve(AllMembers = true)]
    public class BarcodeView : UIView
    {
        private readonly AVCaptureVideoPreviewLayer previewLayer;
        private readonly CAShapeLayer shapeLayer;

        internal BarcodeView(AVCaptureVideoPreviewLayer previewLayer, CAShapeLayer shapeLayer) : base()
        {
            this.previewLayer = previewLayer;
            this.shapeLayer = shapeLayer;

            this.BackgroundColor = UIColor.Black;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (this.previewLayer is not null)
            {
                this.previewLayer.Frame = this.Layer.Bounds;
            }

            if (this.shapeLayer is not null)
            {
                this.shapeLayer.Position = new CGPoint(this.Layer.Bounds.Width / 2, this.Layer.Bounds.Height / 2);
            }

            var previewLayerConnection = this.previewLayer?.Connection;
            if (previewLayerConnection is not null && previewLayerConnection.SupportsVideoOrientation)
            {
                if (UIDevice.CurrentDevice.CheckSystemVersion(13, 0))
                {
                    var interfaceOrientation = this.Window.WindowScene?.InterfaceOrientation;
                    previewLayerConnection.VideoOrientation = GetVideoOrientation(interfaceOrientation);
                }
                else
                {
                    // TODO
                    previewLayerConnection.VideoOrientation = AVCaptureVideoOrientation.Portrait;
                }
            }
        }

        private static AVCaptureVideoOrientation GetVideoOrientation(UIInterfaceOrientation? interfaceOrientation)
        {
            switch (interfaceOrientation)
            {
                case UIInterfaceOrientation.LandscapeLeft:
                    return AVCaptureVideoOrientation.LandscapeLeft;
                case UIInterfaceOrientation.LandscapeRight:
                    return AVCaptureVideoOrientation.LandscapeRight;
                case UIInterfaceOrientation.PortraitUpsideDown:
                    return AVCaptureVideoOrientation.PortraitUpsideDown;
                default:
                    return AVCaptureVideoOrientation.Portrait;
            }
        }
    }
}