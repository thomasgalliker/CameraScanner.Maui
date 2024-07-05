using AVFoundation;
using CoreMedia;
using UIKit;

internal static class DeviceAutomaticVideoZoomFactor
{
    public static float? GetDefaultCameraZoom2(AVCaptureDevice captureDevice, float minimumCodeSize)
    {
        var deviceMinimumFocusDistance = (float)captureDevice.MinimumFocusDistance;
        if (deviceMinimumFocusDistance == -1)
        {
            return null;
            // throw new Exception(nameof(Errors.MinimumFocusDistanceUnknown));
        }

        Console.WriteLine($"Video Zoom Factor: using device: {captureDevice}");
        Console.WriteLine($"Video Zoom Factor: device minimum focus distance: {deviceMinimumFocusDistance}");

        var formatDimensions = (captureDevice.ActiveFormat.FormatDescription as CMVideoFormatDescription).Dimensions;
        var rectOfInterestWidth = (double)formatDimensions.Height / (double)formatDimensions.Width;

        var deviceFieldOfView = captureDevice.ActiveFormat.VideoFieldOfView;
        var minimumSubjectDistanceForCode =
            MinimumSubjectDistanceForCode(deviceFieldOfView, minimumCodeSize, (float)rectOfInterestWidth);

        Console.WriteLine($"Video Zoom Factor: minimum subject distance: {minimumSubjectDistanceForCode}");

        if (minimumSubjectDistanceForCode >= deviceMinimumFocusDistance)
        {
            return null;
        }

        var zoomFactor = deviceMinimumFocusDistance / minimumSubjectDistanceForCode;
        return zoomFactor;
    }

    private static float MinimumSubjectDistanceForCode(float fieldOfView, float minimumCodeSize, float previewFillPercentage)
    {
        var radians = DegreesToRadians(fieldOfView / 2);
        var filledCodeSize = minimumCodeSize / previewFillPercentage;
        return filledCodeSize / (float)Math.Tan(radians);
    }

    private static float DegreesToRadians(float degrees)
    {
        return degrees * (float)Math.PI / 180;
    }


    /// <summary>
    /// Returns the default camera zoom.
    /// </summary>
    /// <param name="captureDevice">The capture device.</param>
    /// <param name="previewFillPercentage">fill xx% of preview window</param>
    /// <param name="minimumTargetObjectSize">min width of the object in mm</param>
    public static float GetDefaultCameraZoom(AVCaptureDevice captureDevice, float previewFillPercentage = 0.6f,
        float minimumTargetObjectSize = 40f)
    {
        if (!UIDevice.CurrentDevice.CheckSystemVersion(15, 0))
        {
            return (float)captureDevice.VideoZoomFactor;
        }

        // Set camera zoom depending on device's minimum focus distance as per Apple's recommendation
        // for scanning barcodes.
        // Adapted from https://stackoverflow.com/questions/74381985/choosing-suitable-camera-for-barcode-scanning-when-using-avcapturedevicetypebuil
        // and https://forums.developer.apple.com/forums/thread/715568
        //
        // Example final VideoZoomFactors:
        // iPhone 14 Pro and 15 Pro (20cm focus distance): ~2.0
        // iPhone 13 Pro (~15cm focus distance): ~1.5
        // iPhone 12 and below: 1.0 - ~1.2
        var minimumFocusDistance = captureDevice.MinimumFocusDistance.ToInt32();
        var videoFieldOfView = captureDevice.ActiveFormat.VideoFieldOfView;
        // const float previewFillPercentage = 0.6f; //
        // const float minimumTargetObjectSize = 40.0f; //
        var filledTargetObjectSize = minimumTargetObjectSize / previewFillPercentage;
        var radians = double.DegreesToRadians(videoFieldOfView);
        var minimumSubjectDistance = filledTargetObjectSize / Math.Tan(radians / 2.0); // Field of view

        var zoomFactor = (float)(minimumFocusDistance / minimumSubjectDistance);
        return zoomFactor;

        // if (minimumSubjectDistance < minimumFocusDistance)
        // {
        //
        //     CaptureDeviceLock(captureDevice, () =>
        //     {
        //         captureDevice.VideoZoomFactor = zoomFactor;
        //         this.cameraView.CurrentZoomFactor = zoomFactor;
        //     });
        // }
    }
}