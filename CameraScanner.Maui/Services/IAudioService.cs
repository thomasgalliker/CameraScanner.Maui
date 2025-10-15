namespace CameraScanner.Maui
{
    public interface IAudioService
    {
        /// <summary>
        /// Gets the singleton instance of <see cref="IAudioService"/>.
        /// </summary>
        public static IAudioService Current { get; } = AudioService.Current;

        float Volume { get; set; }

        void SetSource(Stream audioStream);

        void Play();

        void Stop();

        void Reset();
    }
}