namespace CameraScanner.Maui.Utils
{
    internal class AsyncSemaphore
    {
        private static readonly Task CompletedTask = Task.FromResult(true);
        private readonly Queue<TaskCompletionSource<bool>> waiters = new Queue<TaskCompletionSource<bool>>();
        private int currentCount;

        public AsyncSemaphore(int initialCount)
        {
            if (initialCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(initialCount));
            }

            this.currentCount = initialCount;
        }

        public Task WaitAsync()
        {
            lock (this.waiters)
            {
                if (this.currentCount > 0)
                {
                    --this.currentCount;
                    return CompletedTask;
                }

                var waiter = new TaskCompletionSource<bool>();
                this.waiters.Enqueue(waiter);
                return waiter.Task;
            }
        }

        public void Release()
        {
            TaskCompletionSource<bool> toRelease = null;
            lock (this.waiters)
            {
                if (this.waiters.Count > 0)
                {
                    toRelease = this.waiters.Dequeue();
                }
                else
                {
                    ++this.currentCount;
                }
            }

            toRelease?.SetResult(true);
        }
    }
}