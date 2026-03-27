using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Refresher
{
    /*
    =========================================================================
    TASK.RUN / STARTNEW VARIANTS - SOLID REFERENCE EXAMPLE

    This file demonstrates:

    1. Task.Run with:
       - void-returning lambda
       - value-returning lambda
       - method group
       - multi-statement lambda
       - stored task awaited later
       - async lambda
       - cancellation token

    2. Task.Factory.StartNew with:
       - normal usage
       - LongRunning
       - explicit scheduler
       - async-lambda pitfall (Task<Task>)

    3. Differences between:
       - CPU-bound vs I/O-bound work
       - ThreadPool work vs dedicated long-running worker
       - await vs fire-and-forget style mistakes

    =========================================================================
    BIG IDEAS

    Task.Run(...)
    - Queues work to the ThreadPool
    - Best default for finite CPU-bound work
    - Simpler and safer than Task.Factory.StartNew for common use

    Task.Factory.StartNew(...)
    - Lower-level API
    - More options, more ways to misuse
    - Use when you intentionally need advanced scheduling behavior

    TaskCreationOptions.LongRunning
    - Signals that the work is long-lived / blocking / worker-like
    - Scheduler may use a dedicated thread instead of normal ThreadPool behavior
    - Better fit for near-infinite consumer loops or daemon-style workers

    =========================================================================
    IMPORTANT RULES

    Rule 1:
    - Do NOT wrap naturally async I/O APIs in Task.Run unless you have a very
      specific reason.

      Good:
          await httpClient.GetStringAsync(url);

      Usually unnecessary:
          await Task.Run(() => httpClient.GetStringAsync(url));

    Rule 2:
    - Use Task.Run mainly for CPU-bound work that should not block the caller.

    Rule 3:
    - Use StartNew(..., LongRunning, ...) only when you intentionally want
      long-lived dedicated worker behavior.

    Rule 4:
    - Be careful with async lambdas in StartNew.
      StartNew(async () => ...) often gives Task<Task>, not Task.

    =========================================================================
    */

    public class TaskrunVariants
    {
        public static async Task Exec()
        {
            Console.WriteLine("=== Task.Run Variants Deep Dive ===");
            Console.WriteLine();

            await DemoTaskRunVoidLambdaAsync();
            await DemoTaskRunReturnValueAsync();
            await DemoTaskRunMethodGroupAsync();
            await DemoTaskRunMultiStatementAsync();
            await DemoTaskRunStoreThenAwaitAsync();
            await DemoTaskRunAsyncLambdaAsync();
            await DemoTaskRunWithCancellationAsync();
            await DemoStartNewBasicAsync();
            await DemoStartNewLongRunningAsync();
            await DemoStartNewAsyncLambdaPitfallAsync();
            await DemoCpuBoundVsIoBoundAsync();

            Console.WriteLine("=== End of demo ===");
        }

        // =========================================================
        // 1. Task.Run with a void-returning lambda
        // =========================================================
        private static async Task DemoTaskRunVoidLambdaAsync()
        {
            Console.WriteLine("----- 1. Task.Run with void lambda -----");

            /*
            This is the simplest form:

                await Task.Run(() => HeavyComputation());

            Meaning:
            - queue CPU work to the ThreadPool
            - get back a Task
            - await completion
            */

            await Task.Run(() =>
            {
                Console.WriteLine($"Void lambda running on thread {Environment.CurrentManagedThreadId}");
                HeavyComputation(iterations: 2_000_000);
            });

            Console.WriteLine("Void lambda finished");
            Console.WriteLine();
        }

        // =========================================================
        // 2. Task.Run with a return value
        // =========================================================
        private static async Task DemoTaskRunReturnValueAsync()
        {
            Console.WriteLine("----- 2. Task.Run returning a value -----");

            /*
            If the lambda returns a value, Task.Run returns Task<T>.

                int result = await Task.Run(() => ComputeChecksum(...));

            This is common for CPU-bound calculations whose result you need.
            */

            int checksum = await Task.Run(() =>
            {
                Console.WriteLine($"Value lambda running on thread {Environment.CurrentManagedThreadId}");
                return ComputeChecksum(10_000);
            });

            Console.WriteLine($"Checksum = {checksum}");
            Console.WriteLine();
        }

        // =========================================================
        // 3. Task.Run with method groups
        // =========================================================
        private static async Task DemoTaskRunMethodGroupAsync()
        {
            Console.WriteLine("----- 3. Task.Run with method groups -----");

            /*
            Method group syntax is cleaner when you already have named methods.

                await Task.Run(DoCpuWork);
                int x = await Task.Run(ComputeAnswer);

            This is equivalent to lambda wrapping in many cases.
            */

            await Task.Run(DoCpuWork);

            int result = await Task.Run(ComputeAnswer);

            Console.WriteLine($"Method group result = {result}");
            Console.WriteLine();
        }

        // =========================================================
        // 4. Task.Run with multi-statement lambda
        // =========================================================
        private static async Task DemoTaskRunMultiStatementAsync()
        {
            Console.WriteLine("----- 4. Task.Run with multi-statement lambda -----");

            /*
            Multi-statement lambda is useful when the work needs setup, loops,
            multiple local variables, and a final return value.
            */

            long sum = await Task.Run(() =>
            {
                Console.WriteLine($"Multi-statement lambda on thread {Environment.CurrentManagedThreadId}");

                long total = 0;
                for (int i = 1; i <= 100_000; i++)
                {
                    total += i;
                }

                return total;
            });

            Console.WriteLine($"Sum = {sum}");
            Console.WriteLine();
        }

        // =========================================================
        // 5. Start task now, await later
        // =========================================================
        private static async Task DemoTaskRunStoreThenAwaitAsync()
        {
            Console.WriteLine("----- 5. Store task first, await later -----");

            /*
            Pattern:

                Task<int> task = Task.Run(...);
                // do other work
                int result = await task;

            This is useful when you want overlap between the started task and
            some other independent work.
            */

            Task<int> backgroundTask = Task.Run(() =>
            {
                Console.WriteLine($"Stored task started on thread {Environment.CurrentManagedThreadId}");
                Thread.Sleep(300); // blocking simulation for CPU-thread example
                return 12345;
            });

            Console.WriteLine("Main flow continues while background task is running...");

            // Simulate other independent work in the caller
            await Task.Delay(150);

            int result = await backgroundTask;
            Console.WriteLine($"Stored task result = {result}");
            Console.WriteLine();
        }

        // =========================================================
        // 6. Task.Run with async lambda
        // =========================================================
        private static async Task DemoTaskRunAsyncLambdaAsync()
        {
            Console.WriteLine("----- 6. Task.Run with async lambda -----");

            /*
            This is legal:

                await Task.Run(async () =>
                {
                    await Task.Delay(200);
                    return 999;
                });

            Because Task.Run understands async lambdas and gives you a properly
            unwrapped Task<T>.

            Use this only when you really want the WHOLE workflow to begin from
            a ThreadPool work item.

            For naturally async I/O, this is often unnecessary.
            */

            int result = await Task.Run(async () =>
            {
                Console.WriteLine($"Async lambda started on thread {Environment.CurrentManagedThreadId}");

                await Task.Delay(250);

                Console.WriteLine($"Async lambda resumed on thread {Environment.CurrentManagedThreadId}");
                return 999;
            });

            Console.WriteLine($"Async lambda result = {result}");
            Console.WriteLine();
        }

        // =========================================================
        // 7. Task.Run with cancellation token
        // =========================================================
        private static async Task DemoTaskRunWithCancellationAsync()
        {
            Console.WriteLine("----- 7. Task.Run with cancellation -----");

            /*
            Task.Run has an overload that accepts a CancellationToken.

            Important nuance:
            - passing the token to Task.Run helps cancel scheduling / association
            - BUT your delegate must still cooperate by checking the token

            Cancellation in .NET is cooperative, not forced termination.
            */

            using var cts = new CancellationTokenSource();
            cts.CancelAfter(400);

            try
            {
                int result = await Task.Run(() =>
                {
                    Console.WriteLine($"Cancelable work on thread {Environment.CurrentManagedThreadId}");

                    int total = 0;
                    for (int i = 0; i < 20; i++)
                    {
                        cts.Token.ThrowIfCancellationRequested();

                        Thread.Sleep(50); // simulate chunked CPU/blocking work
                        total += i;
                    }

                    return total;
                }, cts.Token);

                Console.WriteLine($"Cancelable result = {result}");
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Task.Run work was canceled.");
            }

            Console.WriteLine();
        }

        // =========================================================
        // 8. Task.Factory.StartNew basic usage
        // =========================================================
        private static async Task DemoStartNewBasicAsync()
        {
            Console.WriteLine("----- 8. Task.Factory.StartNew basic -----");

            /*
            StartNew is lower-level than Task.Run.

            Most of the time, this:

                Task.Run(...)

            is preferred over:

                Task.Factory.StartNew(...)

            But StartNew allows explicit options and scheduler selection.

            Here is a basic usage:
            */

            Task<int> task = Task.Factory.StartNew(
                () =>
                {
                    Console.WriteLine($"StartNew basic on thread {Environment.CurrentManagedThreadId}");
                    return ComputeChecksum(5_000);
                },
                CancellationToken.None,
                TaskCreationOptions.None,
                TaskScheduler.Default);

            int result = await task;
            Console.WriteLine($"StartNew result = {result}");
            Console.WriteLine();
        }

        // =========================================================
        // 9. StartNew with LongRunning
        // =========================================================
        private static async Task DemoStartNewLongRunningAsync()
        {
            Console.WriteLine("----- 9. StartNew with LongRunning -----");

            /*
            This is the important pattern you asked about.

                Task.Factory.StartNew(
                    () => InfiniteLoop(),
                    CancellationToken.None,
                    TaskCreationOptions.LongRunning,
                    TaskScheduler.Default);

            Meaning:
            - this work is long-lived / worker-like
            - scheduler may use a dedicated thread
            - better fit than occupying a normal ThreadPool worker forever

            This is NOT the default pattern for ordinary short CPU jobs.
            */

            using var cts = new CancellationTokenSource();

            Task worker = Task.Factory.StartNew(
                () => DedicatedWorkerLoop(cts.Token),
                cts.Token,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);

            // Let it run for a short while
            await Task.Delay(700);

            Console.WriteLine("Requesting worker cancellation...");
            cts.Cancel();

            try
            {
                await worker;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Long-running worker stopped cooperatively.");
            }

            Console.WriteLine();
        }

        // =========================================================
        // 10. StartNew async-lambda pitfall
        // =========================================================
        private static async Task DemoStartNewAsyncLambdaPitfallAsync()
        {
            Console.WriteLine("----- 10. StartNew async-lambda pitfall -----");

            /*
            This is a classic pitfall:

                Task<Task<int>> outer = Task.Factory.StartNew(async () => { ... });

            Because the delegate itself returns Task<int>, StartNew wraps that,
            so the result becomes Task<Task<int>>.

            You then must unwrap it manually, or use Task.Run instead.

            Compare:

                Task<int> good = Task.Run(async () => { ... });

            vs

                Task<Task<int>> awkward = Task.Factory.StartNew(async () => { ... });

            This is one major reason Task.Run is safer for modern code.
            */

            Task<Task<int>> outerTask = Task.Factory.StartNew(async () =>
            {
                Console.WriteLine($"StartNew async lambda outer started on thread {Environment.CurrentManagedThreadId}");
                await Task.Delay(200);
                return 777;
            });

            Console.WriteLine($"outerTask type = {outerTask.GetType().Name}");

            // First await unwraps outer task and gives inner task
            Task<int> innerTask = await outerTask;

            // Second await gets final result
            int result = await innerTask;

            Console.WriteLine($"Unwrapped StartNew async result = {result}");

            // Better pattern:
            int cleaner = await Task.Run(async () =>
            {
                await Task.Delay(100);
                return 888;
            });

            Console.WriteLine($"Task.Run async lambda result = {cleaner}");
            Console.WriteLine();
        }

        // =========================================================
        // 11. CPU-bound vs I/O-bound comparison
        // =========================================================
        private static async Task DemoCpuBoundVsIoBoundAsync()
        {
            Console.WriteLine("----- 11. CPU-bound vs I/O-bound guidance -----");

            /*
            CPU-bound example:
            - expensive computation
            - Task.Run is appropriate if you want it off the caller thread

            I/O-bound example:
            - network, DB, file, timer waits
            - prefer naturally async APIs directly
            - usually DO NOT wrap in Task.Run
            */

            // CPU-bound offload
            int cpuResult = await Task.Run(() =>
            {
                Console.WriteLine($"CPU-bound work on thread {Environment.CurrentManagedThreadId}");
                return ExpensiveCpuOperation(3_000_000);
            });

            Console.WriteLine($"CPU-bound result = {cpuResult}");

            // Naturally async I/O simulation
            string ioResult = await SimulateIoAsync("orders.json");
            Console.WriteLine(ioResult);

            // Usually unnecessary wrapper around already-async I/O:
            string wrappedIoResult = await Task.Run(async () =>
            {
                // This works, but is usually extra overhead for no good reason.
                return await SimulateIoAsync("customers.json");
            });

            Console.WriteLine($"Wrapped I/O result = {wrappedIoResult}");
            Console.WriteLine();
        }

        // =========================================================
        // SUPPORT METHODS
        // =========================================================

        private static void HeavyComputation(int iterations)
        {
            long total = 0;
            for (int i = 0; i < iterations; i++)
            {
                total += i % 7;
            }

            Console.WriteLine($"HeavyComputation finished, total = {total}");
        }

        private static int ComputeChecksum(int n)
        {
            int checksum = 0;
            for (int i = 1; i <= n; i++)
            {
                checksum ^= i;
            }

            return checksum;
        }

        private static void DoCpuWork()
        {
            Console.WriteLine($"DoCpuWork on thread {Environment.CurrentManagedThreadId}");
            HeavyComputation(1_500_000);
        }

        private static int ComputeAnswer()
        {
            Console.WriteLine($"ComputeAnswer on thread {Environment.CurrentManagedThreadId}");
            return 42;
        }

        private static void DedicatedWorkerLoop(CancellationToken token)
        {
            Console.WriteLine($"Dedicated worker started on thread {Environment.CurrentManagedThreadId}");

            int heartbeat = 0;

            while (true)
            {
                token.ThrowIfCancellationRequested();

                Thread.Sleep(150); // blocking worker loop simulation
                heartbeat++;

                Console.WriteLine($"Worker heartbeat {heartbeat} on thread {Environment.CurrentManagedThreadId}");
            }
        }

        private static int ExpensiveCpuOperation(int iterations)
        {
            int total = 0;
            for (int i = 0; i < iterations; i++)
            {
                total += (i * 31) % 13;
            }

            return total;
        }

        private static async Task<string> SimulateIoAsync(string resourceName)
        {
            await Task.Delay(250);
            return $"Loaded {resourceName} asynchronously";
        }
    }

    /*
    =========================================================================
    QUICK SUMMARY

    1. Default choice for finite CPU work:
        await Task.Run(() => HeavyComputation());

    2. Return a value:
        int result = await Task.Run(() => Compute());

    3. Start now, await later:
        Task<int> t = Task.Run(() => Compute());
        // do other work
        int x = await t;

    4. Async lambda:
        int x = await Task.Run(async () =>
        {
            await Task.Delay(100);
            return 5;
        });

    5. Cancellation:
        await Task.Run(() => WorkWithToken(token), token);

    6. Dedicated worker / near-infinite loop:
        Task.Factory.StartNew(
            () => WorkerLoop(token),
            token,
            TaskCreationOptions.LongRunning,
            TaskScheduler.Default);

    7. Beware:
        Task.Factory.StartNew(async () => ...)
        can produce Task<Task>

    =========================================================================
    PRACTICAL DECISION GUIDE

    Use Task.Run when:
    - the work is CPU-bound
    - the work is finite
    - you want the simplest correct solution

    Use StartNew(...LongRunning...) when:
    - the work is long-lived
    - the work is blocking / worker-loop style
    - you intentionally want dedicated-thread-like behavior

    Avoid wrapping already-async I/O in Task.Run when:
    - the API already returns Task / Task<T>
    - you are not solving a real blocking problem

    =========================================================================
    */
}