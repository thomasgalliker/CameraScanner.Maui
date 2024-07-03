namespace CameraScanner.Maui
{
    internal class SyncHelper
    {
        private const int NotRunning = 0;
        private const int Running = 1;
        private int currentState;

        public async Task<bool> RunOnceAsync(Func<Task> task)
        {
            var run = Interlocked.CompareExchange(ref this.currentState, Running, NotRunning) == NotRunning;
            if (run)
            {
                // The given task is only executed if we pass this atomic CompareExchange call,
                // which switches the current state flag from 'not running' to 'running'.

                try
                {
                    await task();
                }
                finally
                {
                    Interlocked.Exchange(ref this.currentState, NotRunning);
                }
            }
            else
            {
                // All other method calls which can't make it into the critical section
                // are just returned immediately.
            }

            return run;
        }

        public bool RunOnce(Action action)
        {
            var run = Interlocked.CompareExchange(ref this.currentState, Running, NotRunning) == NotRunning;
            if (run)
            {
                // The given task is only executed if we pass this atomic CompareExchange call,
                // which switches the current state flag from 'not running' to 'running'.

                try
                {
                    action();
                }
                finally
                {
                    Interlocked.Exchange(ref this.currentState, NotRunning);
                }
            }
            else
            {
                // All other method calls which can't make it into the critical section
                // are just returned immediately.
            }

            return run;
        }
    }
}

