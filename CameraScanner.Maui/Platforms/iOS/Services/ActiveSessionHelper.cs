using System.Diagnostics;
using AVFoundation;

namespace CameraScanner.Maui.Platforms.Services
{
    internal class ActiveSessionHelper
    {
        internal static void InitializeSession(AVAudioSessionCategory category, AVAudioSessionMode mode, AVAudioSessionCategoryOptions categoryOptions, SessionLifetime sessionLifetime)
        {
            var audioSession = AVAudioSession.SharedInstance();

            var error = audioSession.SetCategory(category, mode, categoryOptions);
            if (error is not null)
            {
                Trace.TraceError("InitializeSession failed to SetCategory: {0}", error);
            }

            error = audioSession.SetActive(true, GetSessionSetActiveOptions(sessionLifetime));
            if (error is not null)
            {
                Trace.TraceError("InitializeSession failed to SetActive(true): {0}", error);
            }
        }

        public static void FinishSession(SessionLifetime sessionLifetime)
        {
            if (sessionLifetime is not SessionLifetime.KeepSessionAlive)
            {
                var audioSession = AVAudioSession.SharedInstance();

                var error = audioSession.SetActive(false, GetSessionSetActiveOptions(sessionLifetime));
                if (error is not null)
                {
                    Trace.TraceError("FinishSession failed to SetActive(false): {0}", error);
                }
            }
        }

        private static AVAudioSessionSetActiveOptions GetSessionSetActiveOptions(SessionLifetime sessionLifetime)
        {
            if (sessionLifetime is SessionLifetime.EndSessionAndNotifyOthers)
            {
                return AVAudioSessionSetActiveOptions.NotifyOthersOnDeactivation;
            }

            return 0;
        }
    }

    public enum SessionLifetime
    {
        /// <summary>
        /// Keep the audio session alive after stopping.
        /// </summary>
        KeepSessionAlive,

        /// <summary>
        /// End the audio session after stopping.
        /// </summary>
        EndSession,

        /// <summary>
        /// End the audio session after stopping and notify other audio sessions.
        /// </summary>
        EndSessionAndNotifyOthers
    }
}