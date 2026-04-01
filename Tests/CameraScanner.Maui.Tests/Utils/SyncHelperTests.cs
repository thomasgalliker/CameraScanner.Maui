using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CameraScanner.Maui.Utils;
using FluentAssertions;
using Xunit;
using Xunit.Abstractions;

namespace CameraScanner.Maui.Tests.Utils
{
    public class SyncHelperTests
    {
        private readonly ITestOutputHelper testOutputHelper;

        public SyncHelperTests(ITestOutputHelper testOutputHelper)
        {
            this.testOutputHelper = testOutputHelper;
        }

        [Fact]
        public void ShouldRunOnce_IsRunningFalse()
        {
            // Arrange
            var syncHelper = new SyncHelper();

            // Act
            var isRunning = syncHelper.IsRunning;

            // Assert
            isRunning.Should().BeFalse();
        }

        [Fact]
        public void ShouldRunOnce_IsRunningTrue()
        {
            // Arrange
            var isRunning = false;
            var syncHelper = new SyncHelper();

            // Act
            syncHelper.RunOnce(() =>
            {
                isRunning = syncHelper.IsRunning;
            });

            // Assert
            isRunning.Should().BeTrue();
        }

        [Fact]
        public async Task ShouldRunOnce_WithoutReturnValue()
        {
            // Arrange
            var counter = 0;
            var parallelTasks = 64;
            var syncHelper = new SyncHelper();
            using var ready = new CountdownEvent(parallelTasks);
            using var start = new ManualResetEventSlim(false);

            // Act
            var tasks = Enumerable.Range(1, parallelTasks)
                .Select(id => Task.Run(() =>
                {
                    ready.Signal();
                    start.Wait();

                    syncHelper.RunOnce(() =>
                    {
                        Thread.Sleep(100);
                        var value = Interlocked.Increment(ref counter);
                        this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={value}");
                    });
                }))
                .ToList();
            ready.Wait();
            start.Set();
            await Task.WhenAll(tasks);

            // Assert
            counter.Should().Be(1);
        }

        [Fact]
        public async Task ShouldRunOnce_WithReturnValue()
        {
            // Arrange
            var counter = 0;
            var parallelTasks = 64;
            var syncHelper = new SyncHelper();
            using var ready = new CountdownEvent(parallelTasks);
            using var start = new ManualResetEventSlim(false);

            // Act
            var tasks = Enumerable.Range(1, parallelTasks)
                .Select(id => Task.Run(() =>
                {
                    ready.Signal();
                    start.Wait();

                    return syncHelper.RunOnce(() =>
                    {
                        Thread.Sleep(100);
                        var value = Interlocked.Increment(ref counter);
                        this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={value}");
                        return value;
                    });
                }))
                .ToList();
            ready.Wait();
            start.Set();
            var results = await Task.WhenAll(tasks);

            // Assert
            counter.Should().Be(1);
            results.Should().HaveCount(parallelTasks);
            results.Should().AllSatisfy(i => i.Should().Be(1));
        }

        [Fact]
        public async Task ShouldRunOnceAsync_WithoutReturnValue()
        {
            // Arrange
            var counter = 0;
            var parallelTasks = 64;
            var syncHelper = new SyncHelper();
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var ready = 0;

            // Act
            var tasks = Enumerable.Range(1, parallelTasks)
                .Select(id => Task.Run(async () =>
                {
                    Interlocked.Increment(ref ready);
                    await start.Task;

                    await syncHelper.RunOnceAsync(async () =>
                    {
                        await Task.Delay(100);
                        var value = Interlocked.Increment(ref counter);
                        this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={value}");
                    });
                }))
                .ToList();
            SpinWait.SpinUntil(() => Volatile.Read(ref ready) == parallelTasks, TimeSpan.FromSeconds(5)).Should().BeTrue();
            start.SetResult();
            await Task.WhenAll(tasks);

            // Assert
            counter.Should().Be(1);
        }

        [Fact]
        public async Task ShouldRunOnceAsync_WithReturnValue()
        {
            // Arrange
            var counter = 0;
            var parallelTasks = 64;
            var syncHelper = new SyncHelper();
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var ready = 0;

            // Act
            var tasks = Enumerable.Range(1, parallelTasks)
                .Select(id => Task.Run(async () =>
                {
                    Interlocked.Increment(ref ready);
                    await start.Task;

                    return await syncHelper.RunOnceAsync(async () =>
                    {
                        await Task.Delay(100);
                        var value = Interlocked.Increment(ref counter);
                        this.testOutputHelper.WriteLine($"Run #{id}: \t\tcounter={value}");
                        return value;
                    });
                }))
                .ToList();
            SpinWait.SpinUntil(() => Volatile.Read(ref ready) == parallelTasks, TimeSpan.FromSeconds(5)).Should().BeTrue();
            start.SetResult();
            var results = await Task.WhenAll(tasks);

            // Assert
            counter.Should().Be(1);
            results.Should().HaveCount(parallelTasks);
            results.Should().AllSatisfy(i => i.Should().Be(1));
        }
    }
}