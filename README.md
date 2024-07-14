# CameraScanner.Maui
[![Version](https://img.shields.io/nuget/v/CameraScanner.Maui.svg)](https://www.nuget.org/packages/CameraScanner.Maui)  [![Downloads](https://img.shields.io/nuget/dt/CameraScanner.Maui.svg)](https://www.nuget.org/packages/CameraScanner.Maui)

This library offers camera preview and barcode scanning functionality for .NET MAUI apps using native platform APIs with **Google ML Kit** and **Apple Vision framework**.

### Download and Install CameraScanner.Maui
This library is available on NuGet: https://www.nuget.org/packages/CameraScanner.Maui
Use the following command to install Plugin.FirebasePushNotifications using NuGet package manager console:

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

### API Usage
The following documentation guides you trough the most important use cases of this library. Not all aspects are covered. If you think there is something important missing here, feel free to open a new issue.

This documentation only demonstrates the use of CameraScanner.Maui within a XAML and MVVM based app. Of course, the code also runs in C# and code-behind UIs.

#### Use CameraView in XAML
In order to add the camera preview control `CameraView` to your xaml page, you need to add the xml namespace in the root of your xaml file:
```xaml
xmlns:c="http://camerascanner.maui"
```

Then, you can add `CameraView` to your xaml UI.
```xaml
<c:CameraView />
```
There are several bindable properties in `CameraView` in order to configure and control the camera preview.

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

### Contribution
Contributors welcome! If you find a bug or you want to propose a new feature, feel free to do so by opening a new issue on github.com.

### Links
- https://developers.google.com/ml-kit/vision/barcode-scanning
- https://developer.apple.com/documentation/visionkit
- https://orcascan.com/tools/free-barcode-generator
- https://barcode.tec-it.com/