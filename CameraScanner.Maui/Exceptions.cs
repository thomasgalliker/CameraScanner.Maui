namespace CameraScanner.Maui
{
    internal static class Exceptions
    {
        internal static Exception NotImplementedInReferenceAssembly()
        {
            return new NotImplementedException(
                "This functionality is not implemented for the current platform. " +
                "You should reference the NuGet package from your main application project in order to reference the platform-specific implementation.");
        }
    }
}