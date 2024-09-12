using System.Runtime.CompilerServices;
using CameraScanner.Maui;

[assembly: InternalsVisibleTo("CameraScanner.Maui.Tests")]
[assembly: InternalsVisibleTo("MauiSampleApp")]
[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

[assembly: XmlnsDefinition("http://camerascanner.maui", "CameraScanner.Maui")]
[assembly: XmlnsDefinition("http://camerascanner.maui", "CameraScanner.Maui.Controls")]

[assembly: Preserve(typeof(CameraView), AllMembers = true)]

#if ANDROID
[assembly: Preserve(typeof(AndroidX.Camera.View.LifecycleCameraController), AllMembers = true)]
[assembly: Preserve(typeof(AndroidX.Camera.Video.Recorder), AllMembers = true)]
#endif