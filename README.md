# CameraScanner.Maui
[![Version](https://img.shields.io/nuget/v/CameraScanner.Maui.svg)](https://www.nuget.org/packages/CameraScanner.Maui) [![Downloads](https://img.shields.io/nuget/dt/CameraScanner.Maui.svg)](https://www.nuget.org/packages/CameraScanner.Maui) [![Buy Me a Coffee](https://img.shields.io/badge/support-buy%20me%20a%20coffee-FFDD00)](https://buymeacoffee.com/thomasgalliker)

This library offers camera preview and barcode scanning functionality for .NET MAUI apps using native platform APIs with **Google ML Kit** and **Apple VisionKit**.

### Download and Install CameraScanner.Maui
This library is available on NuGet: https://www.nuget.org/packages/CameraScanner.Maui
Use the following command to install CameraScanner.Maui using NuGet package manager console:

    PM> Install-Package CameraScanner.Maui

You can use this library in any .NET MAUI project compatible to .NET 8 and higher.

#### App Setup
1. This plugin provides an extension method for MauiAppBuilder `UseCameraScanner` which ensure proper startup and initialization.
   Call this method within your `MauiProgram` just as demonstrated in the [CameraDemoApp](https://github.com/thomasgalliker/CameraScanner.Maui/tree/develop/Samples):
   ```csharp
   var builder = MauiApp.CreateBuilder()
       .UseMauiApp<App>()
       .UseCameraScanner();
   ```
2. Add camera permission for Android in your `AndroidManifest.xml` file.
   Add the following `uses-permission` entry inside of the `manifest` node:
   ```xml
   <uses-permission android:name="android.permission.CAMERA" />
   ```
3. Add camera permission for iOS in your `Info.plist` file.
   Add the following permission inside of the `dict` node:
   ```xml
   <key>NSCameraUsageDescription</key>
   <string>Required to display camera preview and scan barcodes.</string>
   ```
   Permission strings can also be localized on iOS using lproj-folders and InfoPlist.strings files. Read [this](https://learn.microsoft.com/en-us/dotnet/maui/fundamentals/localization) if you're interested in this topic.

### Sample App
In the **Samples** folder of this repository, you will find the **CameraDemoApp**, which demonstrates the features of CameraScanner.Maui. To debug, clone the repository and run the sample app directly in your development environment.

You can also download the sample app from the Google Play Store and the App Store using the links below:

<a href="https://play.google.com/store/apps/details?id=ch.superdev.camerascanner">
    <img src="https://raw.githubusercontent.com/thomasgalliker/CameraScanner.Maui/refs/heads/develop/Images/GooglePlay/en_badge_web_generic.png" height="44px" alt="Get it on Google Play" />
</a>
<a href="https://apps.apple.com/ch/app/camera-scanner-sdk/id6752654209">
    <img src="https://raw.githubusercontent.com/thomasgalliker/CameraScanner.Maui/refs/heads/develop/Images/AppStore/download-on-the-app-store.svg" height="44px" alt="Download on the App Store">
</a>

### API Usage
The following documentation guides you trough the most important use cases of this library. Not all aspects are covered. If you think there is something important missing here, feel free to open a new issue.

This documentation only demonstrates the use of CameraScanner.Maui within a XAML and MVVM based app. Of course, the code also runs in C# and code-behind UIs.

#### Use CameraView in XAML
In order to add the camera preview control `CameraView` to your xaml page, you need to add the xml namespace in the root of your xaml file:
```xaml
xmlns:c="http://camerascanner.maui"
```

Then, you can add `CameraView` to your xaml UI. Set CameraEnabled="True" in order to start the camera preview.
```xaml
<c:CameraView CameraEnabled="True" />
```
There are several bindable properties in `CameraView` in order to configure and control the camera preview.

#### Configure CameraView

| Property                    | Description                                                                                                                                                                                                                                                                                                                                                                                                                                       |
|-----------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|
| `AutoDisconnectHandler`     | Defines if the platform handler is automatically disconnected or if `Handler.DisconnectHandler();` is called manually. Default: `true` for (automatically disconnected). This property only exists for apps with TFM `net8.0`.                                                                                                                                                                                                                    |
| `VibrationOnDetected`       | Enables or disables vibration on successful barcode detection. Default: `false`                                                                                                                                                                                                                                                                                                                                                                   |
| `CameraEnabled`             | Enables or disables the camera preview. Default: `false`                                                                                                                                                                                                                                                                                                                                                                                          |
| `PauseScanning`             | Pauses barcode scanning.                                                                                                                                                                                                                                                                                                                                                                                                                          |
| `ForceInverted`             | Forces scanning of inverted barcodes. Reduces performance significantly. Android only.                                                                                                                                                                                                                                                                                                                                                            |
| `PoolingInterval`           | Enables pooling of detections for better detection of multiple barcodes at once. Value in milliseconds. Default: 0 (disabled).                                                                                                                                                                                                                                                                                                                    |
| `TorchOn`                   | Enables or disables the torch.                                                                                                                                                                                                                                                                                                                                                                                                                    |
| `TapToFocusEnabled`         | Disables or enables tap-to-focus.                                                                                                                                                                                                                                                                                                                                                                                                                 |
| `CameraFacing`              | Select front or back facing camera. Default: `CameraFacing.Back`                                                                                                                                                                                                                                                                                                                                                                                  |
| `CaptureQuality`            | Set the capture quality for the image analysis. Recommended and default value is Medium. Use the highest values for more precision or lower for fast scanning.                                                                                                                                                                                                                                                                                    |
| `BarcodeFormats`            | Set the enabled barcode formats. Default: `BarcodeFormats.All`.                                                                                                                                                                                                                                                                                                                                                                                   |
| `AimMode`                   | Disables or enables aim mode. When enabled only barcode that is in the center of the preview will be detected.                                                                                                                                                                                                                                                                                                                                    |
| `ViewfinderMode`            | Disables or enables viewfinder mode. When enabled only barcode that is visible in the preview will be detected.                                                                                                                                                                                                                                                                                                                                   |
| `CaptureNextFrame`          | Captures the next frame from the camera feed.                                                                                                                                                                                                                                                                                                                                                                                                     |
| `BarcodeDetectionFrameRate` | Specifies the frequency at which frames are processed for barcode detection. Default: null (no frame skipping) Example: If the value is null, 0 or 1, every frame from the camera preview is analyzed. If the value is 2, every 2nd frame from the camera preview is analyzed. If the value is 3, every 3rd frame from the camera preview is analyzed.                                                                                            |
| `RequestZoomFactor`         | Setting this value changes the zoom factor of the camera. Value has to be between MinZoomFactor and MaxZoomFactor. More info: iOS/macOS - https://developer.apple.com/documentation/avfoundation/avcapturedevice/zoom Android - https://developer.android.com/reference/kotlin/androidx/camera/view/CameraController#setZoomRatio(float) Only logical multi-camera is supported - https://developer.android.com/media/camera/camera2/multi-camera |
| `CurrentZoomFactor`         | Returns current zoom factor value.                                                                                                                                                                                                                                                                                                                                                                                                                |
| `MinZoomFactor`             | Returns minimum zoom factor for current camera.                                                                                                                                                                                                                                                                                                                                                                                                   |
| `MaxZoomFactor`             | Returns maximum zoom factor for current camera.                                                                                                                                                                                                                                                                                                                                                                                                   |
| `DeviceSwitchZoomFactors`   | Returns zoom factors that switch between camera lenses. Supported on iOS only.                                                                                                                                                                                                                                                                                                                                                                    |

#### Ask for camera permission
Before your app is allowed to access the camera stream, the user has to give runtime permission to access the camera. This library provides the interface `ICameraPermissions` to check if the permission is already given and/or to ask for permission. 

Access `ICameraPermissions` via static singleton instance `CameraPermissions.Current` or inject it using dependency injection.


- `IBarcodeScanner` tbd

### Supported Barcode Formats
The following barcode formats are supported on the underlying platforms:

| Barcode Format       | Supported on iOS       | Supported on Android     |
|----------------------|------------------------|--------------------------|
| Aztec                | :white_check_mark:     | :white_check_mark:       |
| Code128              | :white_check_mark:     | :white_check_mark:       |
| Code39               | :white_check_mark:     | :white_check_mark:       |
| Code93               | :white_check_mark:     | :white_check_mark:       |
| DataMatrix           | :white_check_mark:     | :white_check_mark:       |
| Ean13                | :white_check_mark:     | :white_check_mark:       |
| Ean8                 | :white_check_mark:     | :white_check_mark:       |
| ITF                  | :white_check_mark:     | :white_check_mark:       |
| Pdf417               | :white_check_mark:     | :white_check_mark:       |
| QR                   | :white_check_mark:     | :white_check_mark:       |
| UPC_A                | :white_check_mark:     | :white_check_mark:       |
| UPC_E                | :white_check_mark:     | :white_check_mark:       |
| Codabar              | :white_check_mark: *1) | :white_check_mark:       |
| GS1DataBar           | :white_check_mark: *1) | :white_large_square:     |
| GS1DataBarExpanded   | :white_check_mark: *1) | :white_large_square:     |
| GS1DataBarLimited    | :white_check_mark: *1) | :white_large_square:     |
| I2of5                | :white_check_mark:     | :white_large_square:     |
| MicroQR              | :white_check_mark: *1) | :white_large_square:     |
| MicroPdf417          | :white_check_mark: *1) | :white_large_square:     |

_*1) Supported on iOS 15 and later._

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.

### Links
- https://developers.google.com/ml-kit/vision/barcode-scanning
- https://developer.apple.com/documentation/visionkit
- https://orcascan.com/tools/free-barcode-generator
- https://barcode.tec-it.com/
