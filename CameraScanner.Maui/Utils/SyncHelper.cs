using System.Diagnostics;

namespace CameraScanner.Maui.Utils
{
    internal class SyncHelper
    {
        private static readonly object NoResult = new();
        private readonly object syncRoot = new();
        private Task<object>? currentExecutionTask;
        private bool currentExecutionHasResult;

        /// <summary>
        /// Indicates if the current instance of <seealso cref="SyncHelper"/> is currently running.
        /// </summary>
        public bool IsRunning
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.currentExecutionTask != null;
                }
            }
        }

        /// <summary>
        /// Runs the given <paramref name="action"/> only once at a time.
        /// </summary>
        /// <param name="action">The synchronous action.</param>
        public void RunOnce(Action action)
        {
            if (!this.TryBeginExecution(hasResult: false, out var execution))
            {
                return;
            }

            var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
            Debug.WriteLine($"RunOnce: Task {id} started");

            try
            {
                action();
                execution.SetResult(NoResult);
            }
            catch (Exception ex)
            {
                execution.SetException(ex);
                throw;
            }
            finally
            {
                Debug.WriteLine($"RunOnce: Task {id} finished");
                this.EndExecution(execution.Task);
            }
        }

        /// <summary>
        /// Runs the given <paramref name="task"/> only once at a time.
        /// </summary>
        /// <param name="task">The asynchronous task.</param>
        public async Task RunOnceAsync(Func<Task> task)
        {
            if (!this.TryBeginExecution(hasResult: false, out var execution))
            {
                return;
            }

            var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
            Debug.WriteLine($"RunOnceAsync: Task {id} started");

            try
            {
                await task().ConfigureAwait(false);
                execution.SetResult(NoResult);
            }
            catch (Exception ex)
            {
                execution.SetException(ex);
                throw;
            }
            finally
            {
                Debug.WriteLine($"RunOnceAsync: Task {id} finished");
                this.EndExecution(execution.Task);
            }
        }

        /// <summary>
        /// Runs the given <paramref name="function"/> only once at a time.
        /// </summary>
        /// <param name="function">The synchronous function which returns a result of type <typeparamref name="T"/>.</param>
        public T RunOnce<T>(Func<T> function)
        {
            while (true)
            {
                var joinMode = this.TryJoinOrBeginResultExecution(out var executionTask, out var execution);
                if (joinMode == JoinMode.WaitForResult)
                {
                    return (T)executionTask.GetAwaiter().GetResult();
                }

                if (joinMode == JoinMode.WaitForCompletion)
                {
                    executionTask.GetAwaiter().GetResult();
                    continue;
                }

                var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
                Debug.WriteLine($"RunOnce: Task {id} started");

                try
                {
                    var result = function();
                    execution.SetResult(result!);
                    return result;
                }
                catch (Exception ex)
                {
                    execution.SetException(ex);
                    throw;
                }
                finally
                {
                    Debug.WriteLine($"RunOnce: Task {id} finished");
                    this.EndExecution(execution.Task);
                }
            }
        }

        /// <summary>
        /// Runs the given <paramref name="task"/> only once at a time.
        /// </summary>
        /// <param name="task">The asynchronous task which returns a result of type <typeparamref name="T"/>.</param>
        public async Task<T> RunOnceAsync<T>(Func<Task<T>> task)
        {
            while (true)
            {
                var joinMode = this.TryJoinOrBeginResultExecution(out var executionTask, out var execution);
                if (joinMode == JoinMode.WaitForResult)
                {
                    return (T)await executionTask.ConfigureAwait(false);
                }

                if (joinMode == JoinMode.WaitForCompletion)
                {
                    await executionTask.ConfigureAwait(false);
                    continue;
                }

                var id = $"{Guid.NewGuid():N}".Substring(0, 5).ToUpperInvariant();
                Debug.WriteLine($"RunOnceAsync: Task {id} started");

                try
                {
                    var result = await task().ConfigureAwait(false);
                    execution.SetResult(result!);
                    return result;
                }
                catch (Exception ex)
                {
                    execution.SetException(ex);
                    throw;
                }
                finally
                {
                    Debug.WriteLine($"RunOnceAsync: Task {id} finished");
                    this.EndExecution(execution.Task);
                }
            }
        }

        private bool TryBeginExecution(bool hasResult, out TaskCompletionSource<object> execution)
        {
            lock (this.syncRoot)
            {
                if (this.currentExecutionTask != null)
                {
                    execution = null!;
                    return false;
                }

                execution = this.CreateExecutionSource();
                this.currentExecutionTask = execution.Task;
                this.currentExecutionHasResult = hasResult;
                return true;
            }
        }

        private JoinMode TryJoinOrBeginResultExecution(out Task<object> executionTask, out TaskCompletionSource<object> execution)
        {
            lock (this.syncRoot)
            {
                var currentExecutionTask = this.currentExecutionTask;
                if (currentExecutionTask == null)
                {
                    execution = this.CreateExecutionSource();
                    this.currentExecutionTask = execution.Task;
                    this.currentExecutionHasResult = true;
                    executionTask = execution.Task;
                    return JoinMode.BeganExecution;
                }

                executionTask = currentExecutionTask;
                execution = null!;
                return this.currentExecutionHasResult
                    ? JoinMode.WaitForResult
                    : JoinMode.WaitForCompletion;
            }
        }

        private void EndExecution(Task<object> executionTask)
        {
            lock (this.syncRoot)
            {
                if (ReferenceEquals(this.currentExecutionTask, executionTask))
                {
                    this.currentExecutionTask = null;
                    this.currentExecutionHasResult = false;
                }
            }
        }

        private TaskCompletionSource<object> CreateExecutionSource()
        {
            return new TaskCompletionSource<object>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        private enum JoinMode
        {
            BeganExecution,
            WaitForCompletion,
            WaitForResult,
        }
    }
}
