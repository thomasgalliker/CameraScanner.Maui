using Android.Media;
using Stream = System.IO.Stream;

namespace CameraScanner.Maui.Platforms.Services
{
    public class AudioService : IAudioService
    {
        private const float DefaultVolume = 1.0f;
        private readonly MediaPlayer player;
        private byte[] audioBytes;
        private MemoryStream stream;
        private float volume;

        public AudioService()
        {
            this.player = new MediaPlayer();

            const AudioUsageKind audioUsageKind = AudioUsageKind.Alarm;

            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                var audioAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)?
                    .SetUsage(audioUsageKind)?
                    .Build();

                if (audioAttributes is not null)
                {
                    this.player.SetAudioAttributes(audioAttributes);
                }
            }
            else
            {
                var streamType = global::Android.Media.Stream.System;

                switch (audioUsageKind)
                {
                    case AudioUsageKind.Media:
                        streamType = global::Android.Media.Stream.Music;
                        break;
                    case AudioUsageKind.Alarm:
                        streamType = global::Android.Media.Stream.Alarm;
                        break;
                    case AudioUsageKind.Notification:
                        streamType = global::Android.Media.Stream.Notification;
                        break;
                    case AudioUsageKind.VoiceCommunication:
                        streamType = global::Android.Media.Stream.VoiceCall;
                        break;
                    case AudioUsageKind.Unknown:
                        break;
                }

                this.player.SetAudioStreamType(streamType);
            }

            this.Volume = DefaultVolume;
        }

        public void SetSource(Stream audioStream)
        {
            ArgumentNullException.ThrowIfNull(audioStream);

            if (OperatingSystem.IsAndroidVersionAtLeast(23))
            {
                using var memoryStream = new MemoryStream();
                audioStream.CopyTo(memoryStream);
                this.audioBytes = memoryStream.ToArray();
            }
            else
            {
                throw new NotSupportedException();
            }

            this.player.Reset();

            this.PrepareAudioSource();
        }

        private void PrepareAudioSource()
        {
            if (this.audioBytes is not byte[] data)
            {
                throw new ArgumentException("audio source is not set");
            }

            if (OperatingSystem.IsAndroidVersionAtLeast(23))
            {
                var stream = new MemoryStream(data);
                var mediaSource = new StreamMediaDataSource(stream);
                this.player.SetDataSource(mediaSource);
            }
            else
            {
                throw new NotSupportedException();
            }

            this.player.Prepare();
        }

        public void Play()
        {
            if (this.player.IsPlaying)
            {
                this.player.Pause();
                this.player.SeekTo(0);
            }

            this.player.Start();
        }

        public void Stop()
        {
            if (this.player.IsPlaying)
            {
                this.player.Pause();
            }

            this.player.SeekTo(0);
        }

        public float Volume
        {
            get => this.volume;
            set
            {
                if (Math.Abs(this.volume - value) > 0.0001f)
                {
                    this.SetVolume(this.volume = value, balance: 0f);
                }
            }
        }

        private void SetVolume(float volume, float balance)
        {
            volume = Math.Clamp(volume, 0, 1);
            balance = Math.Clamp(balance, -1, 1);

            var leftVolume = volume * (balance <= 0f ? 1f : 1f - balance);
            var rightVolume = volume * (balance >= 0f ? 1f : 1f + balance);

            this.player.SetVolume(leftVolume, rightVolume);
        }

        public void Reset()
        {
            this.Stop();

            this.player.Reset();

            this.audioBytes = null;
        }
    }
}