using AVFoundation;
using CameraScanner.Maui.Extensions;
using Foundation;

namespace CameraScanner.Maui.Platforms.Services
{
    public class AudioService : IAudioService
    {
        private const float DefaultVolume = 1.0f;
        private AVAudioPlayer player;
        private float volume;
        private const SessionLifetime SessionLifetime = Services.SessionLifetime.EndSession;

        public AudioService()
        {
            this.Volume = DefaultVolume;
        }

        public void SetSource(Stream audioStream)
        {
            ArgumentNullException.ThrowIfNull(audioStream);

            if (this.player is AVAudioPlayer player)
            {
                ActiveSessionHelper.FinishSession(SessionLifetime);
                this.Stop();
                player.Dispose();
            }

            audioStream.Rewind();

            var data = NSData.FromStream(audioStream);
            if (data == null)
            {
                throw new ArgumentNullException(nameof(audioStream), "Unable to convert audioStream to NSData.");
            }

            if (data.Length == 0)
            {
                throw new ArgumentException("Unable to create AVAudioPlayer from empty data.", nameof(audioStream));
            }

            this.player = AVAudioPlayer.FromData(data)
                          ?? throw new ArgumentException("Unable to create AVAudioPlayer from data.", nameof(audioStream));

            this.PreparePlayer();
        }

        private void PreparePlayer()
        {
            ActiveSessionHelper.InitializeSession(AVAudioSessionCategory.Playback, AVAudioSessionMode.Default, AVAudioSessionCategoryOptions.MixWithOthers, SessionLifetime);

            // this.player.EnableRate = true;
            this.player.PrepareToPlay();
            this.player.Volume = this.Volume;
        }

        public void Play()
        {
            if (this.player is not AVAudioPlayer player)
            {
                return;
            }

            if (player.Playing)
            {
                player.Pause();
                player.CurrentTime = 0;
            }

            player.Play();
        }

        public void Stop()
        {
            if (this.player is not AVAudioPlayer player)
            {
                return;
            }

            player.Stop();
            player.CurrentTime = 0d;
        }

        /// <summary>
        /// Gets or sets the playback volume 0 to 1 where 0 is no-sound and 1 is full volume.
        /// </summary>
        public float Volume
        {
            get => this.volume;
            set
            {
                if (Math.Abs(this.volume - value) > 0.0001f &&
                    this.player is AVAudioPlayer player)
                {
                    player.Volume = (float)Math.Clamp(value, 0d, 1d);
                }

                this.volume = value;
            }
        }

        public void Reset()
        {
            ActiveSessionHelper.FinishSession(SessionLifetime);
            this.player?.Stop();
            this.player?.Dispose();
            this.player = null;
        }
    }
}