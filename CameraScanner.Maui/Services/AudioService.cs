namespace CameraScanner.Maui
{
    public static class AudioService
    {
        private static readonly Lazy<IAudioService> Implementation = new Lazy<IAudioService>(CreateAudioService, LazyThreadSafetyMode.PublicationOnly);

        public static IAudioService Current
        {
            get => Implementation.Value;
        }

        private static IAudioService CreateAudioService()
        {
#if (ANDROID || IOS)
            return new Platforms.Services.AudioService();
#else
            throw Exceptions.NotImplementedInReferenceAssembly();
#endif
        }
    }
}
