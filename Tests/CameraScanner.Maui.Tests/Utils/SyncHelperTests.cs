using CameraScanner.Maui.Utils;
using FluentAssertions;
using Xunit;

namespace CameraScanner.Maui.Tests.Utils
{
    public class SyncHelperTests
    {
        [Fact]
        public async Task RunOnceAsyncShouldExecuteNonResultTaskOnlyOnce()
        {
            // Arrange
            var helper = new SyncHelper();
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var invocationCount = 0;

            var firstRun = helper.RunOnceAsync(async () =>
            {
                Interlocked.Increment(ref invocationCount);
                started.TrySetResult(true);
                await release.Task;
            });

            await started.Task;

            // Act
            var secondRun = helper.RunOnceAsync(() =>
            {
                Interlocked.Increment(ref invocationCount);
                return Task.CompletedTask;
            });

            release.TrySetResult(true);
            await Task.WhenAll(firstRun, secondRun);

            // Assert
            invocationCount.Should().Be(1);
        }

        [Fact]
        public async Task RunOnceAsyncShouldShareResultAcrossConcurrentCalls()
        {
            // Arrange
            var helper = new SyncHelper();
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var invocationCount = 0;

            Task<int> CreateRun(int result) => helper.RunOnceAsync(async () =>
            {
                Interlocked.Increment(ref invocationCount);
                started.TrySetResult(true);
                await release.Task;
                return result;
            });

            var firstRun = CreateRun(42);
            await started.Task;

            // Act
            var secondRun = CreateRun(99);
            var thirdRun = CreateRun(123);

            release.TrySetResult(true);
            var results = await Task.WhenAll(firstRun, secondRun, thirdRun);

            // Assert
            results.Should().Equal(42, 42, 42);
            invocationCount.Should().Be(1);
        }
    }
}
