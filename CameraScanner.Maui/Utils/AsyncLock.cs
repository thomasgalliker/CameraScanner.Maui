namespace CameraScanner.Maui.Utils
{
    internal class AsyncLock
    {
        private readonly AsyncSemaphore semaphore;
        private readonly Task<Releaser> releaser;

        public AsyncLock()
        {
            this.semaphore = new AsyncSemaphore(1);
            this.releaser = Task.FromResult(new Releaser(this));
        }

        public Task<Releaser> LockAsync()
        {
            var wait = this.semaphore.WaitAsync();
            return wait.IsCompleted
                ? this.releaser
                : wait.ContinueWith((_, state) => new Releaser((AsyncLock)state),
                    this, CancellationToken.None,
                    TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);
        }

        public struct Releaser : IDisposable
        {
            private readonly AsyncLock toRelease;

            internal Releaser(AsyncLock toRelease) { this.toRelease = toRelease; }

            public void Dispose()
            {
                this.toRelease?.semaphore.Release();
            }
        }
    }
}