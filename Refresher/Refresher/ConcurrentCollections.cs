/// <summary>
/// Concurrent Collections — Interview Reference
/// ═══════════════════════════════════════════════════════════════════
///
/// THREAD SAFETY QUICK REFERENCE:
///
/// Collection              Thread-Safe   Notes
/// ─────────────────────────────────────────────────────────────────
/// List<T>                 No            Use lock or ConcurrentBag
/// Dictionary<K,V>         No            Corrupts under concurrent write
/// Queue<T>                No            —
/// Stack<T>                No            —
/// HashSet<T>              No            —
///
/// ConcurrentDictionary    Yes           Lock-free reads, striped locks
/// ConcurrentQueue<T>      Yes           Lock-free FIFO
/// ConcurrentStack<T>      Yes           Lock-free LIFO
/// ConcurrentBag<T>        Yes           Unordered, thread-local storage
/// BlockingCollection<T>   Yes           Producer-consumer with blocking
/// ImmutableList<T>        Yes           Copy-on-write, truly immutable
///
/// INTERVIEW CRITICAL:
///   Dictionary<K,V> under concurrent write = corrupted internal state
///   Not just wrong values — potential infinite loops, exceptions
///   ConcurrentDictionary.GetOrAdd factory is NOT atomic
///   Always use Interlocked for single numeric operations
///   lock(this) is an antipattern — use private readonly object
///
/// ═══════════════════════════════════════════════════════════════════
/// </summary>

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ConcurrentCollections
{
    public class CunCollections
    {
        public static void Exec(string[] args)
        {
            Console.WriteLine("Concurrent Collections — Interview Reference\n");

            DemonstrateDictionaryDanger();
            DemonstrateConcurrentDictionary();
            DemonstrateConcurrentQueue();
            DemonstrateConcurrentStack();
            DemonstrateConcurrentBag();
            DemonstrateBlockingCollection();
            DemonstrateImmutableCollections();
            DemonstrateInterlocked();
            DemonstrateLockPatterns();
        }

        // ═══════════════════════════════════════════════════════════════
        // THE DANGER OF NON-CONCURRENT DICTIONARY
        // ═══════════════════════════════════════════════════════════════
        //
        // Dictionary<K,V> is NOT thread-safe.
        // Concurrent reads during a write cause:
        //   - Corrupted hash buckets
        //   - KeyNotFoundException on existing keys
        //   - Infinite loops in internal traversal
        //   - NullReferenceException
        //
        // This is not a "sometimes wrong value" bug.
        // It's a "corrupts the entire data structure" bug.
        //
        // INTERVIEW: What happens if two threads write to Dictionary simultaneously?
        //   The internal array of Entry[] can be corrupted during resize.
        //   The linked list chains for collision resolution can loop infinitely.
        //   Result is undefined behavior, not just stale data.
        //
        static void DemonstrateDictionaryDanger()
        {
            Console.WriteLine("── DICTIONARY DANGER ──────────────────────────────────");

            // This code demonstrates WHY you need ConcurrentDictionary
            // DO NOT use Dictionary<K,V> across threads without locking

            var unsafeDict = new Dictionary<int, string>();
            var safeDict = new ConcurrentDictionary<int, string>();

            // The unsafe version can throw or corrupt
            Console.WriteLine("Dictionary<K,V> — NOT thread-safe:");
            Console.WriteLine("  Concurrent writes → corrupted hash buckets");
            Console.WriteLine("  Concurrent read + write → undefined behavior");
            Console.WriteLine("  Use ConcurrentDictionary or lock for thread safety");

            // The safe version
            Console.WriteLine("\nConcurrentDictionary — thread-safe:");
            Console.WriteLine("  Lock-free reads using volatile reads");
            Console.WriteLine("  Fine-grained striped locking for writes");
            Console.WriteLine("  Multiple buckets can be written simultaneously");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // CONCURRENTDICTIONARY — THE WORKHORSE
        // ═══════════════════════════════════════════════════════════════
        //
        // INTERNALS:
        //   Reads are lock-free — use volatile reads
        //   Writes use striped locking — 16 locks by default (one per bucket group)
        //   Multiple threads can write to different buckets simultaneously
        //   Only threads writing to the SAME bucket contend with each other
        //
        // KEY METHODS:
        //   TryAdd(k, v)              — atomic add, returns false if key exists
        //   TryGetValue(k, out v)     — safe get
        //   TryRemove(k, out v)       — atomic remove, returns removed value
        //   TryUpdate(k, newV, compV) — atomic compare-and-swap update
        //   GetOrAdd(k, v)            — get existing or add new
        //   GetOrAdd(k, factory)      — get existing or compute and add
        //   AddOrUpdate(k, add, update) — atomic upsert
        //
        // CRITICAL: GetOrAdd FACTORY IS NOT ATOMIC
        //   Multiple threads can call the factory simultaneously
        //   Only ONE value will be stored but factory may run multiple times
        //   Do not use for expensive operations without Lazy<T> pattern
        //
        // CONCURRENCYLEVEL:
        //   Constructor parameter — number of concurrent writers
        //   Default: Environment.ProcessorCount
        //   Setting too high wastes memory, too low increases contention
        //
        static void DemonstrateConcurrentDictionary()
        {
            Console.WriteLine("── CONCURRENTDICTIONARY ───────────────────────────────");

            // Basic construction
            var cache = new ConcurrentDictionary<string, decimal>();

            // TryAdd — returns false if key already exists
            bool added = cache.TryAdd("LOAN-001", 350_000m);
            Console.WriteLine($"TryAdd new key:      {added}");      // True
            bool addedAgain = cache.TryAdd("LOAN-001", 400_000m);
            Console.WriteLine($"TryAdd existing key: {addedAgain}"); // False

            // GetOrAdd — get if exists, otherwise add
            decimal amount = cache.GetOrAdd("LOAN-002", 275_000m);
            Console.WriteLine($"\nGetOrAdd new:       {amount}");    // 275000

            decimal existing = cache.GetOrAdd("LOAN-001", 999_999m);
            Console.WriteLine($"GetOrAdd existing:  {existing}");   // 350000 — not overwritten

            // GetOrAdd with factory — FACTORY NOT ATOMIC
            decimal computed = cache.GetOrAdd("LOAN-003", key =>
            {
                // WARNING: this factory can be called by multiple threads
                // simultaneously. Only one result will be stored, but
                // multiple executions can occur. Don't do expensive I/O here
                // without the Lazy<T> pattern.
                Console.WriteLine($"  Factory called for key: {key}");
                return 500_000m;
            });
            Console.WriteLine($"GetOrAdd factory:   {computed}");

            // Lazy<T> pattern — makes factory truly execute once
            var lazyCache = new ConcurrentDictionary<string, Lazy<decimal>>();
            var lazyValue = lazyCache.GetOrAdd("LOAN-004",
                key => new Lazy<decimal>(() =>
                {
                    // This executes only once — Lazy<T> guarantees it
                    return 425_000m; // Simulate expensive DB call
                }));
            Console.WriteLine($"Lazy GetOrAdd:      {lazyValue.Value}");

            // AddOrUpdate — atomic upsert
            decimal updated = cache.AddOrUpdate(
                "LOAN-001",
                addValue: 350_000m,                           // Add with this value if new
                updateValueFactory: (key, oldVal) => oldVal * 1.1m  // Update formula if exists
            );
            Console.WriteLine($"\nAddOrUpdate result: {updated}"); // 350000 * 1.1 = 385000

            // TryUpdate — compare-and-swap
            bool swapped = cache.TryUpdate("LOAN-001", 400_000m, 385_000m);
            Console.WriteLine($"TryUpdate (CAS):    {swapped}"); // True — was 385000

            // TryRemove
            bool removed = cache.TryRemove("LOAN-002", out decimal removedVal);
            Console.WriteLine($"\nTryRemove:          {removed}, value: {removedVal}");

            // Thread-safe enumeration — snapshot semantics
            Console.WriteLine("\nAll entries (snapshot):");
            foreach (var kvp in cache) // Safe to enumerate — takes consistent snapshot
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:C}");

            // Count — approximate under concurrent access
            Console.WriteLine($"\nCount: {cache.Count}");
            Console.WriteLine($"IsEmpty: {cache.IsEmpty}");

            // Concurrent load test
            Console.WriteLine("\nConcurrent writes test:");
            var concurrentDict = new ConcurrentDictionary<int, int>();
            Parallel.For(0, 1000, i =>
            {
                concurrentDict.AddOrUpdate(i % 10, 1, (k, v) => v + 1);
            });
            int total = 0;
            foreach (var v in concurrentDict.Values) total += v;
            Console.WriteLine($"Total increments: {total}"); // Should be 1000
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // CONCURRENTQUEUE<T> — LOCK-FREE FIFO
        // ═══════════════════════════════════════════════════════════════
        //
        // INTERNALS:
        //   Linked list of segments (arrays)
        //   Lock-free using Interlocked CAS operations
        //   Head pointer for dequeue, tail pointer for enqueue
        //   Multiple producers and consumers safe simultaneously
        //
        // USE CASES:
        //   - Task/work queues in producer-consumer patterns
        //   - Message passing between threads
        //   - Loan application processing pipeline
        //   - Event queues in high-throughput systems
        //
        // KEY METHODS:
        //   Enqueue(item)               — always succeeds, no capacity limit
        //   TryDequeue(out item)        — returns false if empty
        //   TryPeek(out item)           — look without removing
        //   Count                       — approximate! not guaranteed exact
        //   IsEmpty                     — more reliable than Count == 0
        //
        // INTERVIEW: Why TryDequeue instead of Dequeue?
        //   Between checking Count > 0 and calling Dequeue,
        //   another thread could have dequeued the item.
        //   TryDequeue is atomic — check and remove in one operation.
        //
        static void DemonstrateConcurrentQueue()
        {
            Console.WriteLine("── CONCURRENTQUEUE<T> ─────────────────────────────────");

            var queue = new ConcurrentQueue<string>();

            // Enqueue — always succeeds
            queue.Enqueue("LOAN-001");
            queue.Enqueue("LOAN-002");
            queue.Enqueue("LOAN-003");

            Console.WriteLine($"Count: {queue.Count}");    // 3
            Console.WriteLine($"IsEmpty: {queue.IsEmpty}"); // False

            // TryPeek — look without removing
            if (queue.TryPeek(out string peeked))
                Console.WriteLine($"Peek: {peeked}");  // LOAN-001

            // TryDequeue — atomic check and remove
            while (queue.TryDequeue(out string item))
                Console.WriteLine($"Dequeued: {item}");

            Console.WriteLine($"After drain — IsEmpty: {queue.IsEmpty}");

            // Producer-consumer pattern
            Console.WriteLine("\nProducer-Consumer pattern:");
            var processingQueue = new ConcurrentQueue<int>();
            var cts = new CancellationTokenSource();
            int processed = 0;

            // Producer task
            var producer = Task.Run(() =>
            {
                for (int i = 0; i < 20; i++)
                {
                    processingQueue.Enqueue(i);
                    Thread.Sleep(1); // Simulate work
                }
            });

            // Consumer task
            var consumer = Task.Run(() =>
            {
                while (!cts.Token.IsCancellationRequested || !processingQueue.IsEmpty)
                {
                    if (processingQueue.TryDequeue(out int item))
                        Interlocked.Increment(ref processed);
                    else
                        Thread.SpinWait(10); // Back off if empty
                }
            });

            producer.Wait();
            cts.CancelAfter(100); // Give consumer time to drain
            consumer.Wait();
            Console.WriteLine($"Processed: {processed} items");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // CONCURRENTSTACK<T> — LOCK-FREE LIFO
        // ═══════════════════════════════════════════════════════════════
        //
        // Lock-free linked list — LIFO ordering
        // Push/Pop operations use Interlocked.CompareExchange
        //
        // USE CASES:
        //   - Undo/redo stacks in concurrent editors
        //   - Work-stealing thread pool implementations
        //   - DFS traversal across multiple threads
        //
        static void DemonstrateConcurrentStack()
        {
            Console.WriteLine("── CONCURRENTSTACK<T> ─────────────────────────────────");

            var stack = new ConcurrentStack<string>();

            stack.Push("First");
            stack.Push("Second");
            stack.Push("Third");

            // PushRange — push multiple atomically
            stack.PushRange(new[] { "Fourth", "Fifth" });

            Console.WriteLine($"Count: {stack.Count}"); // 5

            // TryPop
            if (stack.TryPop(out string top))
                Console.WriteLine($"Popped: {top}"); // "Fifth" — LIFO

            // TryPopRange — pop multiple atomically
            var buffer = new string[3];
            int popped = stack.TryPopRange(buffer);
            Console.WriteLine($"PopRange count: {popped}");
            for (int i = 0; i < popped; i++)
                Console.WriteLine($"  {buffer[i]}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // CONCURRENTBAG<T> — UNORDERED, THREAD-LOCAL OPTIMIZED
        // ═══════════════════════════════════════════════════════════════
        //
        // INTERNALS:
        //   Each thread has its own local storage list
        //   Thread adds to and removes from its own list first (no contention)
        //   When local list is empty, steals from other threads
        //   Order is NOT guaranteed — LIFO tendency but not strict
        //
        // BEST FOR:
        //   Object pooling — multiple producers and consumers
        //   When each thread produces and consumes its own items
        //   Order doesn't matter
        //
        // NOT GOOD FOR:
        //   Producer-consumer where producer != consumer thread
        //   Any situation requiring ordered processing
        //
        static void DemonstrateConcurrentBag()
        {
            Console.WriteLine("── CONCURRENTBAG<T> ───────────────────────────────────");

            var bag = new ConcurrentBag<int>();

            // Multiple producers
            Parallel.For(0, 100, i => bag.Add(i));

            Console.WriteLine($"Bag count: {bag.Count}"); // 100

            // Multiple consumers
            int sum = 0;
            Parallel.For(0, 100, i =>
            {
                if (bag.TryTake(out int item))
                    Interlocked.Add(ref sum, item);
            });

            Console.WriteLine($"Sum: {sum}"); // 4950 (0+1+...+99)
            Console.WriteLine($"IsEmpty after drain: {bag.IsEmpty}");

            // Object pool pattern using ConcurrentBag
            Console.WriteLine("\nObject pool pattern:");
            var pool = new ConcurrentBag<StringBuilder>();

            StringBuilder Rent()
            {
                if (pool.TryTake(out var sb))
                {
                    sb.Clear(); // Reset state
                    return sb;
                }
                return new StringBuilder(256); // Create new if pool empty
            }

            void Return(StringBuilder sb) => pool.Add(sb);

            var borrowed = Rent();
            borrowed.Append("Loan data processed");
            Console.WriteLine($"Pooled SB: {borrowed}");
            Return(borrowed); // Return to pool for reuse

            Console.WriteLine($"Pool size after return: {pool.Count}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // BLOCKINGCOLLECTION<T> — BOUNDED PRODUCER-CONSUMER
        // ═══════════════════════════════════════════════════════════════
        //
        // Wraps any IProducerConsumerCollection (ConcurrentQueue by default)
        // Adds BLOCKING semantics — threads wait when empty/full
        //
        // KEY FEATURES:
        //   BoundedCapacity — limits queue size, blocks producers when full
        //   CompleteAdding() — signals no more items will be added
        //   GetConsumingEnumerable() — IEnumerable that blocks until item
        //                              available, stops when completed
        //
        // USE CASES:
        //   - Pipeline processing with backpressure
        //   - Bounded work queues to prevent memory exhaustion
        //   - Clean producer-consumer shutdown semantics
        //
        // INTERVIEW: Why use BlockingCollection over ConcurrentQueue?
        //   ConcurrentQueue: TryDequeue returns false when empty — you must spin/poll
        //   BlockingCollection: Take() blocks the thread until item available
        //   BlockingCollection enforces capacity limits — prevents OOM under load
        //
        static void DemonstrateBlockingCollection()
        {
            Console.WriteLine("── BLOCKINGCOLLECTION<T> ──────────────────────────────");

            // Bounded queue — backpressure when full
            var pipeline = new BlockingCollection<string>(boundedCapacity: 5);

            // Producer
            var producerTask = Task.Run(() =>
            {
                for (int i = 0; i < 10; i++)
                {
                    string loanId = $"LOAN-{i:D3}";
                    pipeline.Add(loanId); // Blocks if pipeline full (capacity=5)
                    Console.WriteLine($"  Produced: {loanId}");
                }
                pipeline.CompleteAdding(); // Signal no more items
            });

            // Consumer — GetConsumingEnumerable blocks until item or completed
            var consumerTask = Task.Run(() =>
            {
                foreach (string loanId in pipeline.GetConsumingEnumerable())
                {
                    Console.WriteLine($"  Consumed: {loanId}");
                    Thread.Sleep(10); // Simulate processing
                }
                Console.WriteLine("  Consumer: pipeline completed");
            });

            Task.WaitAll(producerTask, consumerTask);
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // IMMUTABLE COLLECTIONS
        // ═══════════════════════════════════════════════════════════════
        //
        // System.Collections.Immutable namespace
        // Truly immutable — modification returns a NEW collection
        // Inherently thread-safe — no mutation possible
        // Structural sharing — new versions share unchanged nodes with old
        //
        // TYPES:
        //   ImmutableList<T>       — O(log n) add/remove/index
        //   ImmutableArray<T>      — O(1) index, no structural sharing
        //   ImmutableDictionary    — O(log n) operations
        //   ImmutableHashSet<T>    — O(log n) operations
        //   ImmutableQueue<T>      — persistent queue
        //   ImmutableStack<T>      — persistent stack
        //
        // WHEN TO USE:
        //   Configuration objects shared across threads
        //   Snapshots of state that must not change
        //   Functional programming patterns
        //   Read-heavy, rarely-updated shared state
        //
        // BUILDER PATTERN for bulk construction:
        //   var builder = ImmutableList.CreateBuilder<T>();
        //   // Add many items
        //   var list = builder.ToImmutable(); // Single allocation
        //
        static void DemonstrateImmutableCollections()
        {
            Console.WriteLine("── IMMUTABLE COLLECTIONS ──────────────────────────────");

            // ImmutableList — add returns NEW list
            var list1 = ImmutableList.Create("LOAN-001", "LOAN-002");
            var list2 = list1.Add("LOAN-003");          // NEW list
            var list3 = list2.Remove("LOAN-001");        // NEW list

            Console.WriteLine($"list1 count: {list1.Count}"); // 2 — unchanged
            Console.WriteLine($"list2 count: {list2.Count}"); // 3
            Console.WriteLine($"list3 count: {list3.Count}"); // 2

            // ImmutableDictionary
            var dict1 = ImmutableDictionary<string, decimal>.Empty;
            var dict2 = dict1.Add("LOAN-001", 350_000m);
            var dict3 = dict2.Add("LOAN-002", 275_000m);
            var dict4 = dict3.SetItem("LOAN-001", 400_000m); // Update

            Console.WriteLine($"\nOriginal LOAN-001: {dict2["LOAN-001"]:C}"); // $350,000
            Console.WriteLine($"Updated  LOAN-001: {dict4["LOAN-001"]:C}");  // $400,000

            // Builder pattern for bulk construction — efficient
            var builder = ImmutableList.CreateBuilder<string>();
            for (int i = 0; i < 5; i++)
                builder.Add($"LOAN-{i:D3}");
            ImmutableList<string> bulkList = builder.ToImmutable();
            Console.WriteLine($"\nBulk built count: {bulkList.Count}");

            // ImmutableArray — value type, better for small fixed arrays
            ImmutableArray<int> arr = ImmutableArray.Create(1, 2, 3, 4, 5);
            ImmutableArray<int> arr2 = arr.Add(6); // NEW array — no sharing
            Console.WriteLine($"\nImmutableArray: [{string.Join(", ", arr2)}]");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // INTERLOCKED — ATOMIC OPERATIONS ON PRIMITIVES
        // ═══════════════════════════════════════════════════════════════
        //
        // CPU-level atomic operations — no lock required
        // Only works on: int, long, float, double, IntPtr, object reference
        //
        // KEY METHODS:
        //   Increment(ref int)        — atomic ++
        //   Decrement(ref int)        — atomic --
        //   Add(ref int, value)       — atomic +=
        //   Exchange(ref T, value)    — atomic swap, returns old value
        //   CompareExchange(ref T, newVal, comparand)
        //     — if current == comparand, set to newVal, return original
        //     — This is the foundation of all lock-free programming
        //   Read(ref long)            — atomic 64-bit read on 32-bit systems
        //
        // INTERVIEW: What is compare-and-swap (CAS)?
        //   Atomic operation: if(current == expected) { current = newVal; }
        //   Returns the original value regardless
        //   Foundation of all lock-free data structures
        //   Used internally by ConcurrentDictionary, ConcurrentQueue etc.
        //
        static void DemonstrateInterlocked()
        {
            Console.WriteLine("── INTERLOCKED — ATOMIC OPS ───────────────────────────");

            int counter = 0;

            // Thread-unsafe increment (for comparison)
            // counter++ is NOT atomic — read, increment, write are 3 ops
            // Two threads can read the same value and both write the same result

            // Thread-safe increment
            Interlocked.Increment(ref counter);
            Interlocked.Increment(ref counter);
            Console.WriteLine($"Atomic counter: {counter}"); // 2

            // Atomic add
            Interlocked.Add(ref counter, 10);
            Console.WriteLine($"After Add(10): {counter}"); // 12

            // Concurrent increment test — no lock needed
            int atomicCount = 0;
            Parallel.For(0, 1000, _ => Interlocked.Increment(ref atomicCount));
            Console.WriteLine($"Parallel increment (no lock): {atomicCount}"); // Always 1000

            // Compare-and-swap — lock-free update pattern
            int original = 0;
            int oldVal = Interlocked.CompareExchange(ref original, 100, 0);
            // If original was 0, set to 100
            Console.WriteLine($"\nCAS: oldVal={oldVal}, original now={original}"); // 0, 100

            // CAS fails if value changed
            int oldVal2 = Interlocked.CompareExchange(ref original, 200, 0);
            // original is now 100, not 0, so CAS fails
            Console.WriteLine($"CAS fail: oldVal={oldVal2}, original={original}"); // 100, 100

            // Exchange — atomic swap
            int prev = Interlocked.Exchange(ref original, 999);
            Console.WriteLine($"Exchange: prev={prev}, new={original}"); // 100, 999

            // Lock-free counter with CAS loop — spin until success
            int lockFreeValue = 0;
            void LockFreeIncrement(ref int val)
            {
                int current, newVal;
                do
                {
                    current = val;
                    newVal = current + 1;
                } while (Interlocked.CompareExchange(ref val, newVal, current) != current);
                // Retry if another thread changed val before our CAS
            }

            Parallel.For(0, 100, _ => LockFreeIncrement(ref lockFreeValue));
            Console.WriteLine($"\nLock-free increment: {lockFreeValue}"); // Always 100
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // LOCK PATTERNS — WHEN YOU MUST LOCK
        // ═══════════════════════════════════════════════════════════════
        //
        // Use concurrent collections and Interlocked first.
        // Lock only when you need to protect a compound operation.
        //
        // LOCK ANTIPATTERNS:
        //   lock(this)          — public lock, can deadlock with external code
        //   lock(typeof(T))     — global lock, extreme contention
        //   lock("string")      — string interning means shared lock globally
        //   lock on value type  — won't compile but know why
        //
        // CORRECT PATTERN:
        //   private readonly object _lock = new object();
        //   lock(_lock) { ... }
        //
        // READER-WRITER LOCK:
        //   ReaderWriterLockSlim — multiple concurrent readers, exclusive writer
        //   Use when: read-heavy, infrequent writes
        //   EnterReadLock / ExitReadLock
        //   EnterWriteLock / ExitWriteLock
        //   EnterUpgradeableReadLock — read then conditionally upgrade to write
        //
        // DEADLOCK CONDITIONS (all four must hold):
        //   1. Mutual exclusion — resource held exclusively
        //   2. Hold and wait — holding one lock while waiting for another
        //   3. No preemption — locks not forcibly released
        //   4. Circular wait — A waits for B waits for A
        //
        // PREVENT DEADLOCK:
        //   Always acquire locks in the same order
        //   Use timeout: Monitor.TryEnter(obj, timeout)
        //   Keep locks short — never do I/O inside a lock
        //
        static void DemonstrateLockPatterns()
        {
            Console.WriteLine("── LOCK PATTERNS ──────────────────────────────────────");

            // Correct lock pattern
            var loanCache = new LoanCache();
            Parallel.For(0, 100, i =>
            {
                loanCache.AddLoan($"LOAN-{i:D3}", i * 1000m);
                _ = loanCache.GetLoan($"LOAN-{i:D3}");
            });
            Console.WriteLine($"Safe cache count: {loanCache.Count}");

            // ReaderWriterLockSlim — read-heavy scenario
            var rwCache = new ReadHeavyCache();
            // Simulate read-heavy workload
            Parallel.For(0, 1000, i =>
            {
                if (i % 100 == 0)
                    rwCache.Update($"KEY-{i}", i * 100m); // 1% writes
                else
                    _ = rwCache.Read("KEY-0");             // 99% reads
            });
            Console.WriteLine($"RW cache: {rwCache.Count} entries");

            // SemaphoreSlim — limit concurrent access
            Console.WriteLine("\nSemaphoreSlim — max 3 concurrent:");
            var semaphore = new SemaphoreSlim(initialCount: 3, maxCount: 3);
            int concurrent = 0;
            int maxConcurrent = 0;

            var tasks = new Task[10];
            for (int i = 0; i < 10; i++)
            {
                int id = i;
                tasks[i] = Task.Run(async () =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        int c = Interlocked.Increment(ref concurrent);
                        Interlocked.Exchange(ref maxConcurrent, Math.Max(maxConcurrent, c));
                        await Task.Delay(10); // Simulate work
                    }
                    finally
                    {
                        Interlocked.Decrement(ref concurrent);
                        semaphore.Release();
                    }
                });
            }
            Task.WaitAll(tasks);
            Console.WriteLine($"Max concurrent: {maxConcurrent}"); // Always <= 3
            Console.WriteLine();

            Console.WriteLine("── ANTIPATTERNS — NEVER DO THESE ─────────────────────");
            Console.WriteLine("lock(this)           — external code can deadlock you");
            Console.WriteLine("lock(typeof(T))      — process-wide contention");
            Console.WriteLine("lock(\"string\")       — interned strings are shared globally");
            Console.WriteLine("I/O inside lock      — holds lock while waiting for disk/network");
            Console.WriteLine("Nested locks differ order — deadlock guaranteed eventually");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // SUPPORTING CLASSES
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Correct lock pattern — private readonly lock object
    /// </summary>
    public class LoanCache
    {
        private readonly Dictionary<string, decimal> _cache = new();
        private readonly object _lock = new object(); // CORRECT: private readonly

        public void AddLoan(string id, decimal amount)
        {
            lock (_lock) // Compound operation — need lock
            {
                if (!_cache.ContainsKey(id))
                    _cache[id] = amount;
            }
        }

        public decimal? GetLoan(string id)
        {
            lock (_lock)
            {
                return _cache.TryGetValue(id, out decimal val) ? val : null;
            }
        }

        public int Count
        {
            get { lock (_lock) { return _cache.Count; } }
        }
    }

    /// <summary>
    /// ReaderWriterLockSlim — optimal for read-heavy workloads
    /// Multiple readers can read simultaneously
    /// Writers get exclusive access
    /// </summary>
    public class ReadHeavyCache
    {
        private readonly Dictionary<string, decimal> _data = new();
        private readonly ReaderWriterLockSlim _rwLock = new ReaderWriterLockSlim();

        public decimal? Read(string key)
        {
            _rwLock.EnterReadLock(); // Multiple threads can hold simultaneously
            try
            {
                return _data.TryGetValue(key, out decimal val) ? val : null;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }

        public void Update(string key, decimal value)
        {
            _rwLock.EnterWriteLock(); // Exclusive — all readers must finish first
            try
            {
                _data[key] = value;
            }
            finally
            {
                _rwLock.ExitWriteLock();
            }
        }

        public int Count
        {
            get
            {
                _rwLock.EnterReadLock();
                try { return _data.Count; }
                finally { _rwLock.ExitReadLock(); }
            }
        }
    }
}