
using System.Diagnostics;


namespace Refresher
{
    /*
    =========================================================================
    ASYNC / AWAIT DEEP DIVE

    This file shows:

    1. Basic async method returning Task<T>
    2. Concurrent async work with Task.WhenAll
    3. CancellationToken usage
    4. Timeout handling
    5. Exception handling in async code
    6. Progress reporting with IProgress<T>
    7. SemaphoreSlim to limit concurrency
    8. Difference between CPU-bound and I/O-bound work
    9. What async/await roughly compiles into internally
    10. Common mistakes and rules

    =========================================================================
    CORE CONCEPTS

    async:
    - Marks a method as using await
    - Usually returns Task, Task<T>, ValueTask, or void (only mainly for event handlers)

    await:
    - Suspends the async method until the awaited Task completes
    - DOES NOT usually block the thread
    - Control returns to caller
    - When the awaited operation completes, the method resumes

    Task:
    - Represents an operation that may complete in the future

    Task<T>:
    - Same, but also produces a result of type T

    =========================================================================
    IMPORTANT RULES

    Rule 1:
    - async does NOT create a new thread by itself

    Rule 2:
    - await does NOT block like Thread.Sleep or Task.Wait

    Rule 3:
    - async/await is best for I/O-bound work
      Examples:
      * HTTP calls
      * file I/O
      * database I/O
      * network operations

    Rule 4:
    - For CPU-bound work, use Task.Run if you intentionally want to offload it

    Rule 5:
    - Avoid async void except for UI event handlers

    Rule 6:
    - Prefer "await" over ".Result" and ".Wait()" to avoid deadlocks and blocking

    =========================================================================
    VERY IMPORTANT INTERNAL MENTAL MODEL

    This method:

        public async Task<int> ExampleAsync()
        {
            int a = await GetNumberAsync();
            return a + 1;
        }

    is conceptually transformed by the compiler into something like:

        - a state machine struct/class is generated
        - local variables become fields in the state machine
        - the method gets a state integer field
        - when await is hit:
            * if the awaited task is already complete, continue immediately
            * otherwise:
                - store current state
                - register continuation
                - return control to caller
        - when awaited task completes:
            * continuation runs
            * state machine resumes from saved state

    So async/await is not magic.
    It is compiler-generated continuation/state-machine logic.

    =========================================================================
    */

    public class AsyncAwait
    {
        // Shared HttpClient should usually be reused.
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        public static async Task Exec()
        {
            Console.WriteLine("=== Async/Await Deep Dive ===");
            Console.WriteLine();

            await DemoSimpleAwaitAsync();
            await DemoConcurrentAwaitAsync();
            await DemoExceptionHandlingAsync();
            await DemoCancellationAsync();
            await DemoTimeoutAsync();
            await DemoProgressAsync();
            await DemoConcurrencyThrottlingAsync();
            await DemoCpuVsIoAsync();
            await DemoInternalMentalModelAsync();
        }

        // =========================================================
        // 1. SIMPLE ASYNC/AWAIT
        // =========================================================
        private static async Task DemoSimpleAwaitAsync()
        {
            Console.WriteLine("----- 1. Simple async/await -----");

            int result = await FetchAndTransformAsync(21);

            Console.WriteLine($"Result = {result}");
            Console.WriteLine();

            /*
            FLOW:

            Main() calls FetchAndTransformAsync(21)
                -> method starts executing synchronously until first await

            await SimulateRemoteCallAsync(...)
                -> if remote task not complete, method yields control
                -> Main can continue later after await completes

            once awaited task finishes
                -> method resumes
                -> returns result inside Task<int>

            Note:
            The caller uses await too, so it naturally unwraps the result.
            */
        }

        private static async Task<int> FetchAndTransformAsync(int input)
        {
            Console.WriteLine("FetchAndTransformAsync started");

            // Simulated I/O wait
            int remoteValue = await SimulateRemoteCallAsync(input);

            Console.WriteLine("FetchAndTransformAsync resumed after await");

            return remoteValue * 2;
        }

        private static async Task<int> SimulateRemoteCallAsync(int value)
        {
            await Task.Delay(500); // non-blocking timer-based async wait
            return value + 10;
        }

        // =========================================================
        // 2. CONCURRENT ASYNC WORK
        // =========================================================
        private static async Task DemoConcurrentAwaitAsync()
        {
            Console.WriteLine("----- 2. Concurrent async with Task.WhenAll -----");

            Stopwatch swSequential = Stopwatch.StartNew();

            int a1 = await SimulateRemoteCallAsync(1);
            int a2 = await SimulateRemoteCallAsync(2);
            int a3 = await SimulateRemoteCallAsync(3);

            swSequential.Stop();

            Console.WriteLine($"Sequential: {a1}, {a2}, {a3} in {swSequential.ElapsedMilliseconds} ms");

            Stopwatch swConcurrent = Stopwatch.StartNew();

            // Start tasks first
            Task<int> t1 = SimulateRemoteCallAsync(1);
            Task<int> t2 = SimulateRemoteCallAsync(2);
            Task<int> t3 = SimulateRemoteCallAsync(3);

            // Await them together
            int[] results = await Task.WhenAll(t1, t2, t3);

            swConcurrent.Stop();

            Console.WriteLine($"Concurrent: {string.Join(", ", results)} in {swConcurrent.ElapsedMilliseconds} ms");
            Console.WriteLine();

            /*
            KEY LESSON:

            This is sequential:

                var r1 = await A();
                var r2 = await B();
                var r3 = await C();

            because B starts only after A finishes, and C starts only after B finishes.

            This is concurrent:

                Task<int> t1 = A();
                Task<int> t2 = B();
                Task<int> t3 = C();
                await Task.WhenAll(t1, t2, t3);

            because all three operations are started before waiting for completion.

            This matters a lot for HTTP calls, DB calls, file reads, etc.
            */
        }

        // =========================================================
        // 3. EXCEPTION HANDLING
        // =========================================================
        private static async Task DemoExceptionHandlingAsync()
        {
            Console.WriteLine("----- 3. Exception handling in async code -----");

            try
            {
                await MethodThatFailsAsync();
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"Caught expected async exception: {ex.Message}");
            }

            Console.WriteLine();

            /*
            KEY LESSON:

            Exceptions thrown inside an async Task method are captured into the Task.

            When you "await" the task:
            - the exception is re-thrown at the await point

            So normal try/catch works naturally around await.

            This is one reason await is cleaner than ContinueWith chains.
            */
        }

        private static async Task MethodThatFailsAsync()
        {
            await Task.Delay(200);
            throw new InvalidOperationException("Something failed asynchronously.");
        }

        // =========================================================
        // 4. CANCELLATION
        // =========================================================
        private static async Task DemoCancellationAsync()
        {
            Console.WriteLine("----- 4. CancellationToken -----");

            using var cts = new CancellationTokenSource();

            Task worker = LongRunningOperationAsync(cts.Token);

            // Cancel after 700ms
            cts.CancelAfter(700);

            try
            {
                await worker;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Operation was canceled.");
            }

            Console.WriteLine();

            /*
            CANCELLATION MODEL:

            - Cancellation in .NET is cooperative
            - You pass a CancellationToken into the async method
            - The method periodically checks token state
            - If canceled, it throws OperationCanceledException
              (often via token.ThrowIfCancellationRequested())

            Cancellation does NOT forcibly kill the method.
            The method must cooperate.
            */
        }

        private static async Task LongRunningOperationAsync(CancellationToken cancellationToken)
        {
            for (int i = 1; i <= 10; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Console.WriteLine($"Working... step {i}");

                // Pass token so delay itself can be canceled
                await Task.Delay(200, cancellationToken);
            }

            Console.WriteLine("Long-running work completed.");
        }

        // =========================================================
        // 5. TIMEOUT
        // =========================================================
        private static async Task DemoTimeoutAsync()
        {
            Console.WriteLine("----- 5. Timeout pattern -----");

            try
            {
                string result = await WithTimeoutAsync(
                    SlowOperationAsync(),
                    TimeSpan.FromMilliseconds(500));

                Console.WriteLine(result);
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"Timeout caught: {ex.Message}");
            }

            Console.WriteLine();

            /*
            Common async pattern:
            - run an operation
            - race it against a timeout
            - if timeout wins, throw

            In modern .NET, Task.WaitAsync(timeout) also exists and is often simpler.
            */
        }

        private static async Task<string> SlowOperationAsync()
        {
            await Task.Delay(1200);
            return "Slow operation completed";
        }

        private static async Task<T> WithTimeoutAsync<T>(Task<T> task, TimeSpan timeout)
        {
            Task delayTask = Task.Delay(timeout);

            Task completed = await Task.WhenAny(task, delayTask);

            if (completed == delayTask)
            {
                throw new TimeoutException("The operation exceeded the timeout.");
            }

            return await task;
        }

        // =========================================================
        // 6. PROGRESS REPORTING
        // =========================================================
        private static async Task DemoProgressAsync()
        {
            Console.WriteLine("----- 6. Progress reporting -----");

            IProgress<int> progress = new Progress<int>(percent =>
            {
                Console.WriteLine($"Progress: {percent}%");
            });

            int total = await ProcessItemsWithProgressAsync(
                Enumerable.Range(1, 5).ToList(),
                progress);

            Console.WriteLine($"Progress demo total = {total}");
            Console.WriteLine();

            /*
            IProgress<T> pattern:
            - async worker reports progress through interface
            - caller decides how to display/use it

            In UI apps, Progress<T> also helps marshal progress callbacks back
            to the captured synchronization context.
            */
        }

        private static async Task<int> ProcessItemsWithProgressAsync(
            List<int> items,
            IProgress<int>? progress)
        {
            int sum = 0;

            for (int i = 0; i < items.Count; i++)
            {
                await Task.Delay(150); // simulate async work
                sum += items[i];

                int percent = (i + 1) * 100 / items.Count;
                progress?.Report(percent);
            }

            return sum;
        }

        // =========================================================
        // 7. CONCURRENCY THROTTLING
        // =========================================================
        private static async Task DemoConcurrencyThrottlingAsync()
        {
            Console.WriteLine("----- 7. Throttling concurrency with SemaphoreSlim -----");

            var inputs = Enumerable.Range(1, 8).ToList();

            List<int> results = await ProcessWithLimitedConcurrencyAsync(inputs, maxConcurrency: 3);

            Console.WriteLine($"Results: {string.Join(", ", results)}");
            Console.WriteLine();

            /*
            Why throttle?

            Suppose you have 500 URLs to fetch.
            Starting 500 requests at once may:
            - overwhelm system resources
            - hit rate limits
            - exhaust sockets
            - overload a remote service

            SemaphoreSlim is a standard way to cap simultaneous async operations.
            */
        }

        private static async Task<List<int>> ProcessWithLimitedConcurrencyAsync(
            List<int> inputs,
            int maxConcurrency)
        {
            using var semaphore = new SemaphoreSlim(maxConcurrency);

            var tasks = inputs.Select(async item =>
            {
                await semaphore.WaitAsync();
                try
                {
                    Console.WriteLine($"Start {item} on thread {Environment.CurrentManagedThreadId}");

                    // Simulated I/O
                    await Task.Delay(300);

                    Console.WriteLine($"End {item} on thread {Environment.CurrentManagedThreadId}");

                    return item * item;
                }
                finally
                {
                    semaphore.Release();
                }
            });

            int[] results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        // =========================================================
        // 8. CPU-BOUND VS I/O-BOUND
        // =========================================================
        private static async Task DemoCpuVsIoAsync()
        {
            Console.WriteLine("----- 8. CPU-bound vs I/O-bound -----");

            // I/O-bound async example
            string ioResult = await SimulateHttpFetchAsync("https://example.com/data");
            Console.WriteLine(ioResult);

            // CPU-bound offloading example
            int cpuResult = await Task.Run(() => ExpensiveCpuCalculation(40_000));
            Console.WriteLine($"CPU result = {cpuResult}");

            Console.WriteLine();

            /*
            I/O-bound:
            - use true async APIs (Task-returning I/O APIs)
            - do NOT wrap already-async I/O in Task.Run unnecessarily

            CPU-bound:
            - if you want to move heavy work off current thread, use Task.Run

            BAD idea:
                await Task.Run(() => httpClient.GetStringAsync(...));

            Better:
                await httpClient.GetStringAsync(...);

            Because HTTP I/O is already naturally asynchronous.
            */
        }

        private static async Task<string> SimulateHttpFetchAsync(string url)
        {
            await Task.Delay(250);
            return $"Fetched data from {url}";
        }

        private static int ExpensiveCpuCalculation(int n)
        {
            // Deliberately CPU-heavy-ish loop
            int total = 0;
            for (int i = 0; i < n; i++)
            {
                total += (i * 31) % 7;
            }
            return total;
        }

        // =========================================================
        // 9. INTERNAL MENTAL MODEL / STATE MACHINE
        // =========================================================
        private static async Task DemoInternalMentalModelAsync()
        {
            Console.WriteLine("----- 9. Internal state-machine mental model -----");

            int value = await TwoStepAsync();

            Console.WriteLine($"TwoStepAsync result = {value}");
            Console.WriteLine();

            /*
            CONSIDER THIS REAL METHOD:

                private static async Task<int> TwoStepAsync()
                {
                    int a = await Step1Async();
                    int b = await Step2Async(a);
                    return b * 10;
                }

            ROUGHLY, THE COMPILER TURNS IT INTO SOMETHING LIKE:

                private struct TwoStepAsyncStateMachine : IAsyncStateMachine
                {
                    public int _state;
                    public AsyncTaskMethodBuilder<int> _builder;

                    private TaskAwaiter<int> _awaiter;
                    private int a;
                    private int b;

                    public void MoveNext()
                    {
                        try
                        {
                            int result;

                            if (_state == 0)
                            {
                                goto ResumeAfterFirstAwait;
                            }
                            if (_state == 1)
                            {
                                goto ResumeAfterSecondAwait;
                            }

                            var awaiter1 = Step1Async().GetAwaiter();
                            if (!awaiter1.IsCompleted)
                            {
                                _state = 0;
                                _awaiter = awaiter1;
                                _builder.AwaitUnsafeOnCompleted(ref awaiter1, ref this);
                                return;
                            }

                            a = awaiter1.GetResult();

                        ResumeAfterFirstAwait:
                            if (_state == 0)
                            {
                                var completedAwaiter1 = _awaiter;
                                _awaiter = default;
                                _state = -1;
                                a = completedAwaiter1.GetResult();
                            }

                            var awaiter2 = Step2Async(a).GetAwaiter();
                            if (!awaiter2.IsCompleted)
                            {
                                _state = 1;
                                _awaiter = awaiter2;
                                _builder.AwaitUnsafeOnCompleted(ref awaiter2, ref this);
                                return;
                            }

                            b = awaiter2.GetResult();

                        ResumeAfterSecondAwait:
                            if (_state == 1)
                            {
                                var completedAwaiter2 = _awaiter;
                                _awaiter = default;
                                _state = -1;
                                b = completedAwaiter2.GetResult();
                            }

                            result = b * 10;
                            _builder.SetResult(result);
                        }
                        catch (Exception ex)
                        {
                            _builder.SetException(ex);
                        }
                    }

                    public void SetStateMachine(IAsyncStateMachine stateMachine) { }
                }

            THAT IS NOT EXACT PRODUCTION CODE.
            But conceptually it is close enough to understand the mechanism.

            MAIN TAKEAWAYS:
            - locals that must survive across await become fields
            - state integer tracks where to resume
            - continuation is registered
            - method returns before completion if await is incomplete
            - later it resumes from the stored state
            */
        }

        private static async Task<int> TwoStepAsync()
        {
            int a = await Step1Async();
            int b = await Step2Async(a);
            return b * 10;
        }

        private static async Task<int> Step1Async()
        {
            await Task.Delay(100);
            return 5;
        }

        private static async Task<int> Step2Async(int x)
        {
            await Task.Delay(100);
            return x + 2;
        }
    }

    /*
    =========================================================================
    COMMON MISTAKES

    1. Blocking on async:
        var x = SomeAsync().Result;
        SomeAsync().Wait();

       Problems:
       - blocks thread
       - can deadlock in some synchronization-context environments
       - loses async scalability

    2. Forgetting to await:
        SomeAsync(); // fire-and-forget unintentionally

       Problem:
       - exceptions may be lost or observed late
       - caller may continue before work completes

    3. Using async void:
        public async void DoWork() { ... }

       Problem:
       - hard to await
       - hard to compose
       - exceptions behave differently

       Prefer:
        public async Task DoWork()

    4. Assuming async means multi-threaded:
       It often does not.
       A lot of async code is about not blocking threads during waits.

    5. Wrapping naturally async APIs in Task.Run unnecessarily:
       This adds overhead and confusion.

    =========================================================================
    CONTEXT CAPTURE NOTE

    In many app models, await captures the current context and resumes there.
    For example:
    - UI thread in desktop apps
    - request context in some server scenarios

    Library code sometimes uses:
        await someTask.ConfigureAwait(false);

    Meaning:
    - do not force resume onto captured context
    - usually improves performance and avoids some deadlock scenarios in libraries

    Example:
        await Task.Delay(100).ConfigureAwait(false);

    In a simple console app, this often matters less than in UI/library code.

    =========================================================================
    EVENT HANDLER EXCEPTION: async void

    async void is mainly acceptable for event handlers, e.g.:

        button.Click += async (sender, e) =>
        {
            await SaveAsync();
        };

    Why?
    - event handler signatures often return void by design

    Outside event handlers, prefer Task-returning methods.

    =========================================================================
    SUMMARY

    async/await gives you:
    - readable asynchronous code
    - non-blocking waits
    - exception propagation that feels synchronous
    - natural composition with Task.WhenAll / WhenAny
    - cancellation and progress patterns
    - scalable I/O programming

    Internally, it is:
    - compiler-generated state machine
    - continuation registration
    - resumption after awaited task completion

    =========================================================================
    */
}