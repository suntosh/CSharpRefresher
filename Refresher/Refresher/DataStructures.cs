namespace Refresher
{

    using System;
    using System.Collections.Generic;
    using System.Linq;

   
        /*
        =========================================================================
        COLLECTIONS OVERVIEW

        Array
        - Fixed size
        - Fast indexed access
        - Best iterators:
          * for
          * foreach
          * Array.ForEach
          * LINQ

        List<T>
        - Dynamic size
        - Fast indexed access
        - Best iterators:
          * for
          * foreach
          * List<T>.ForEach
          * Enumerator
          * LINQ

        Dictionary<TKey, TValue>
        - Key/value storage
        - Fast key lookup
        - Best iterators:
          * foreach over KeyValuePair
          * foreach over Keys / Values
          * Enumerator
          * LINQ

        HashSet<T>
        - Unique items only
        - Fast membership checks
        - No index
        - Best iterators:
          * foreach
          * Enumerator
          * LINQ

        Queue<T>
        - FIFO (first in, first out)
        - Best access:
          * Enqueue / Dequeue / Peek
          * foreach
          * LINQ

        Stack<T>
        - LIFO (last in, first out)
        - Best access:
          * Push / Pop / Peek
          * foreach
          * LINQ

        LinkedList<T>
        - Node-based collection
        - No direct index
        - Best access:
          * foreach
          * manual node traversal
          * LINQ

        =========================================================================
        ITERATION STYLES

        1. for
           - Best when index matters
           - Works well with arrays and List<T>

        2. foreach
           - Best for general collection traversal
           - Works on most IEnumerable collections

        3. while + enumerator
           - Explicit iterator pattern
           - Useful to understand how foreach works internally

        4. Collection-specific traversal
           - Queue: Dequeue loop
           - Stack: Pop loop
           - LinkedList: node-by-node traversal

        5. Lambda / LINQ
           - Where   = filter
           - Select  = transform
           - OrderBy = sort
           - Any     = existence check
           - All     = universal check
           - Count   = count by condition
           - First / FirstOrDefault = pick one
           - GroupBy = group by key

        =========================================================================
        */

        public class DataStructures
        {
            public static void Exec()
            {
                DemoArray();
                DemoList();
                DemoDictionary();
                DemoHashSet();
                DemoQueue();
                DemoStack();
                DemoLinkedList();
                DemoCommonLambdaPatterns();
            }

            // =========================================================
            // 1. ARRAY
            // =========================================================
            private static void DemoArray()
            {
                Console.WriteLine("===== ARRAY =====");

                int[] scores = { 90, 75, 88, 100, 67 };

                // for:
                // Best when you need index-based access.
                Console.WriteLine("-- for loop --");
                for (int i = 0; i < scores.Length; i++)
                {
                    Console.WriteLine($"scores[{i}] = {scores[i]}");
                }

                // foreach:
                // Best when you only need each value.
                Console.WriteLine("-- foreach loop --");
                foreach (int score in scores)
                {
                    Console.WriteLine(score);
                }

                // Array.ForEach:
                // A static helper for arrays only.
                Console.WriteLine("-- Array.ForEach --");
                Array.ForEach(scores, score => Console.WriteLine(score));

                // LINQ:
                // Filter and transform with lambdas.
                Console.WriteLine("-- LINQ --");
                var passedScores = scores.Where(x => x >= 80);
                var curvedScores = scores.Select(x => x + 5);

                Console.WriteLine("Passed scores:");
                foreach (var x in passedScores)
                    Console.WriteLine(x);

                Console.WriteLine("Curved scores:");
                foreach (var x in curvedScores)
                    Console.WriteLine(x);

                Console.WriteLine();
            }

            // =========================================================
            // 2. LIST<T>
            // =========================================================
            private static void DemoList()
            {
                Console.WriteLine("===== LIST<T> =====");

                List<string> names = new List<string> { "Santosh", "Alice", "Bob", "Charlie" };

                // for:
                Console.WriteLine("-- for loop --");
                for (int i = 0; i < names.Count; i++)
                {
                    Console.WriteLine($"names[{i}] = {names[i]}");
                }

                // foreach:
                Console.WriteLine("-- foreach loop --");
                foreach (string name in names)
                {
                    Console.WriteLine(name);
                }

                // List<T>.ForEach:
                // A method specific to List<T>.
                Console.WriteLine("-- List.ForEach --");
                names.ForEach(name => Console.WriteLine(name.ToUpper()));

                // Explicit enumerator:
                // This is close to what foreach uses internally.
                Console.WriteLine("-- explicit enumerator --");
                using (var enumerator = names.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        string current = enumerator.Current;
                        Console.WriteLine(current);
                    }
                }

                // Lambda / LINQ examples:
                Console.WriteLine("-- LINQ --");
                var longNames = names.Where(n => n.Length > 4);
                var initials = names.Select(n => n[0]);
                var sorted = names.OrderBy(n => n);

                Console.WriteLine("Long names:");
                foreach (var n in longNames)
                    Console.WriteLine(n);

                Console.WriteLine("Initials:");
                foreach (var c in initials)
                    Console.WriteLine(c);

                Console.WriteLine("Sorted:");
                foreach (var n in sorted)
                    Console.WriteLine(n);

                Console.WriteLine();
            }

            // =========================================================
            // 3. DICTIONARY<TKey, TValue>
            // =========================================================
            private static void DemoDictionary()
            {
                Console.WriteLine("===== DICTIONARY<TKey, TValue> =====");

                Dictionary<string, int> inventory = new Dictionary<string, int>
            {
                { "Laptop", 5 },
                { "Mouse", 20 },
                { "Keyboard", 12 }
            };

                // foreach over key/value pairs
                Console.WriteLine("-- foreach over pairs --");
                foreach (KeyValuePair<string, int> item in inventory)
                {
                    Console.WriteLine($"{item.Key} => {item.Value}");
                }

                // foreach over keys
                Console.WriteLine("-- foreach over keys --");
                foreach (string key in inventory.Keys)
                {
                    Console.WriteLine($"Key = {key}");
                }

                // foreach over values
                Console.WriteLine("-- foreach over values --");
                foreach (int value in inventory.Values)
                {
                    Console.WriteLine($"Value = {value}");
                }

                // explicit enumerator
                Console.WriteLine("-- explicit enumerator --");
                using (var enumerator = inventory.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        var current = enumerator.Current;
                        Console.WriteLine($"{current.Key} => {current.Value}");
                    }
                }

                // LINQ / lambda examples
                Console.WriteLine("-- LINQ --");
                var lowStock = inventory.Where(kvp => kvp.Value < 10);
                var productNames = inventory.Select(kvp => kvp.Key);
                var totalUnits = inventory.Sum(kvp => kvp.Value);

                Console.WriteLine("Low stock:");
                foreach (var kvp in lowStock)
                    Console.WriteLine($"{kvp.Key} => {kvp.Value}");

                Console.WriteLine("Product names:");
                foreach (var name in productNames)
                    Console.WriteLine(name);

                Console.WriteLine($"Total units = {totalUnits}");
                Console.WriteLine();
            }

            // =========================================================
            // 4. HASHSET<T>
            // =========================================================
            private static void DemoHashSet()
            {
                Console.WriteLine("===== HASHSET<T> =====");

                HashSet<int> uniqueIds = new HashSet<int> { 101, 102, 103, 103, 104 };

                // foreach:
                // HashSet is ideal for unique membership, not indexing.
                Console.WriteLine("-- foreach --");
                foreach (int id in uniqueIds)
                {
                    Console.WriteLine(id);
                }

                // explicit enumerator
                Console.WriteLine("-- explicit enumerator --");
                using (var enumerator = uniqueIds.GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Console.WriteLine(enumerator.Current);
                    }
                }

                // lambda / LINQ
                Console.WriteLine("-- LINQ --");
                var largeIds = uniqueIds.Where(id => id > 102);
                bool contains101 = uniqueIds.Contains(101);
                bool anyAbove200 = uniqueIds.Any(id => id > 200);

                Console.WriteLine("IDs > 102:");
                foreach (var id in largeIds)
                    Console.WriteLine(id);

                Console.WriteLine($"Contains 101 = {contains101}");
                Console.WriteLine($"Any above 200 = {anyAbove200}");
                Console.WriteLine();
            }

            // =========================================================
            // 5. QUEUE<T>
            // =========================================================
            private static void DemoQueue()
            {
                Console.WriteLine("===== QUEUE<T> =====");

                Queue<string> supportTickets = new Queue<string>();
                supportTickets.Enqueue("Ticket-001");
                supportTickets.Enqueue("Ticket-002");
                supportTickets.Enqueue("Ticket-003");

                // foreach:
                // Traverses current contents in queue order without removing items.
                Console.WriteLine("-- foreach --");
                foreach (string ticket in supportTickets)
                {
                    Console.WriteLine(ticket);
                }

                // Peek:
                Console.WriteLine($"Next ticket = {supportTickets.Peek()}");

                // while + Dequeue:
                // This is a processing pattern, not just iteration.
                Console.WriteLine("-- while + Dequeue --");
                while (supportTickets.Count > 0)
                {
                    string current = supportTickets.Dequeue();
                    Console.WriteLine($"Processing {current}");
                }

                // Rebuild queue for LINQ example
                supportTickets.Enqueue("Ticket-010");
                supportTickets.Enqueue("Ticket-011");
                supportTickets.Enqueue("Urgent-999");

                Console.WriteLine("-- LINQ --");
                var urgent = supportTickets.Where(t => t.StartsWith("Urgent"));
                foreach (var t in urgent)
                    Console.WriteLine(t);

                Console.WriteLine();
            }

            // =========================================================
            // 6. STACK<T>
            // =========================================================
            private static void DemoStack()
            {
                Console.WriteLine("===== STACK<T> =====");

                Stack<string> browserHistory = new Stack<string>();
                browserHistory.Push("home");
                browserHistory.Push("products");
                browserHistory.Push("checkout");

                // foreach:
                // Enumerates from top to bottom in stack order.
                Console.WriteLine("-- foreach --");
                foreach (string page in browserHistory)
                {
                    Console.WriteLine(page);
                }

                // Peek:
                Console.WriteLine($"Current page = {browserHistory.Peek()}");

                // while + Pop:
                Console.WriteLine("-- while + Pop --");
                while (browserHistory.Count > 0)
                {
                    string page = browserHistory.Pop();
                    Console.WriteLine($"Back from {page}");
                }

                // rebuild for LINQ example
                browserHistory.Push("home");
                browserHistory.Push("about");
                browserHistory.Push("contact");

                Console.WriteLine("-- LINQ --");
                var pagesWithO = browserHistory.Where(p => p.Contains('o'));
                foreach (var p in pagesWithO)
                    Console.WriteLine(p);

                Console.WriteLine();
            }

            // =========================================================
            // 7. LINKEDLIST<T>
            // =========================================================
            private static void DemoLinkedList()
            {
                Console.WriteLine("===== LINKEDLIST<T> =====");

                LinkedList<int> steps = new LinkedList<int>();
                steps.AddLast(10);
                steps.AddLast(20);
                steps.AddLast(30);
                steps.AddLast(40);

                // foreach:
                Console.WriteLine("-- foreach --");
                foreach (int step in steps)
                {
                    Console.WriteLine(step);
                }

                // manual node traversal:
                // Useful because LinkedList is node-oriented.
                Console.WriteLine("-- manual node traversal --");
                LinkedListNode<int>? node = steps.First;
                while (node != null)
                {
                    Console.WriteLine(node.Value);
                    node = node.Next;
                }

                // LINQ:
                Console.WriteLine("-- LINQ --");
                var doubled = steps.Select(x => x * 2);
                foreach (var x in doubled)
                    Console.WriteLine(x);

                Console.WriteLine();
            }

            // =========================================================
            // 8. COMMON LAMBDA PATTERNS ACROSS COLLECTIONS
            // =========================================================
            private static void DemoCommonLambdaPatterns()
            {
                Console.WriteLine("===== COMMON LAMBDA PATTERNS =====");

                List<Employee> employees = new List<Employee>
            {
                new Employee { Id = 1, Name = "Santosh", Department = "Engineering", Salary = 140000 },
                new Employee { Id = 2, Name = "Alice", Department = "Engineering", Salary = 125000 },
                new Employee { Id = 3, Name = "Bob", Department = "Finance", Salary = 110000 },
                new Employee { Id = 4, Name = "Charlie", Department = "Engineering", Salary = 99000 },
                new Employee { Id = 5, Name = "Diana", Department = "HR", Salary = 95000 }
            };

                // Where = filter
                Console.WriteLine("-- Where --");
                var engineers = employees.Where(e => e.Department == "Engineering");
                foreach (var e in engineers)
                    Console.WriteLine($"{e.Name} - {e.Department}");

                // Select = projection / transformation
                Console.WriteLine("-- Select --");
                var names = employees.Select(e => e.Name);
                foreach (var name in names)
                    Console.WriteLine(name);

                // OrderBy + ThenBy = sorting
                Console.WriteLine("-- OrderBy + ThenBy --");
                var ordered = employees.OrderBy(e => e.Department).ThenBy(e => e.Salary);
                foreach (var e in ordered)
                    Console.WriteLine($"{e.Department} - {e.Name} - {e.Salary}");

                // Any = existence check
                Console.WriteLine("-- Any --");
                bool hasHighEarner = employees.Any(e => e.Salary > 130000);
                Console.WriteLine($"Has salary > 130000 = {hasHighEarner}");

                // All = universal condition
                Console.WriteLine("-- All --");
                bool allAbove90000 = employees.All(e => e.Salary > 90000);
                Console.WriteLine($"All salaries > 90000 = {allAbove90000}");

                // First / FirstOrDefault
                Console.WriteLine("-- FirstOrDefault --");
                var hrEmployee = employees.FirstOrDefault(e => e.Department == "HR");
                Console.WriteLine(hrEmployee != null ? hrEmployee.Name : "none");

                // Count with condition
                Console.WriteLine("-- Count --");
                int engineeringCount = employees.Count(e => e.Department == "Engineering");
                Console.WriteLine($"Engineering count = {engineeringCount}");

                // GroupBy
                Console.WriteLine("-- GroupBy --");
                var grouped = employees.GroupBy(e => e.Department);
                foreach (var group in grouped)
                {
                    Console.WriteLine($"Department: {group.Key}");
                    foreach (var e in group)
                    {
                        Console.WriteLine($"  {e.Name}");
                    }
                }

                // Aggregate-style sums and averages
                Console.WriteLine("-- Sum / Average --");
                decimal totalSalary = employees.Sum(e => e.Salary);
                decimal avgSalary = employees.Average(e => e.Salary);
                Console.WriteLine($"Total salary = {totalSalary}");
                Console.WriteLine($"Average salary = {avgSalary}");

                Console.WriteLine();
            }
        }

        public class Employee
        {
            public int Id { get; set; }
            public string Name { get; set; } = "";
            public string Department { get; set; } = "";
            public decimal Salary { get; set; }
        }
 }


