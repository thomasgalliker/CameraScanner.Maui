using System.Diagnostics;

namespace CameraScanner.Maui.Utils
{
    /// <summary>
    /// Source: https://github.com/thomasgalliker/CrossPlatformLibrary/blob/develop/CrossPlatformLibrary.Shared/Utils/TaskDelayer.cs
    /// </summary>
    internal class TaskDelayer
    {
        private CancellationTokenSource throttleCts = new CancellationTokenSource();

        /// <summary>
        ///     Runs the given <paramref name="action" /> in a background thread with a sliding <paramref name="delay" />.
        /// </summary>
        public Task RunWithDelay(TimeSpan delay, Action action)
        {
            return this.RunWithDelay(delay, () =>
            {
                action();
                return Task.CompletedTask;
            });
        }

        /// <summary>
        ///     Runs the given <paramref name="task" /> in a background thread with a sliding <paramref name="delay" />.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="delay">Sliding delay.</param>
        /// <param name="task">The task to be executed after delay.</param>
        /// <param name="defaultValue">The default value to be returned in case of a cancellation</param>
        /// <returns></returns>
        public Task<T> RunWithDelay<T>(TimeSpan delay, Func<Task<T>> task, Func<T> defaultValue)
        {
            var tcs = new TaskCompletionSource<T>();

            Task.Factory.StartNew(async () =>
            {
                try
                {
                    Interlocked.Exchange(ref this.throttleCts, new CancellationTokenSource()).Cancel();

                    Debug.WriteLine($"RunWithDelay {delay} starts now");
                    await Task.Delay(delay, this.throttleCts.Token)
                        .ContinueWith(async ct =>
                            {
                                try
                                {
                                    var result = await task().ConfigureAwait(false);
                                    tcs.TrySetResult(result);
                                }
                                catch (Exception ex)
                                {
                                    tcs.TrySetException(ex);
                                }
                            },
                            CancellationToken.None,
                            TaskContinuationOptions.OnlyOnRanToCompletion,
                            TaskScheduler.Default);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"RunWithDelay failed with exception: {ex}");
                    tcs.TrySetResult(defaultValue());
                }
            }).ConfigureAwait(false);

            return tcs.Task;
        }

        /// <summary>
        ///     Runs the given <paramref name="task" /> in a background thread with a sliding <paramref name="delay" />.
        /// </summary>
        /// <param name="delay">Sliding delay.</param>
        /// <param name="task">The task to be executed after delay.</param>
        public async Task RunWithDelay(TimeSpan delay, Func<Task> task)
        {
            try
            {
                Interlocked.Exchange(ref this.throttleCts, new CancellationTokenSource()).Cancel();

                Debug.WriteLine($"RunWithDelay {delay} starts now");
                await Task.Delay(delay, this.throttleCts.Token)
                    .ContinueWith(async ct => await task().ConfigureAwait(false),
                        CancellationToken.None,
                        TaskContinuationOptions.OnlyOnRanToCompletion,
                        TaskScheduler.Default);
            }
            catch (TaskCanceledException)
            {
                // Ignore TaskCanceledException
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"RunWithDelay failed with exception: {ex}");
                throw;
            }
        }

        public void Cancel()
        {
            this.throttleCts.Cancel();
        }
    }
}