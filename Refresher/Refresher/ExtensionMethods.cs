/// <summary>
/// Extension Methods — Complete Reference
/// ═══════════════════════════════════════════════════════════════════
///
/// WHAT THEY ARE:
///   Static methods in a static class with 'this' on the first parameter
///   Called as if they were instance methods on that type
///   Do NOT modify the original type — purely syntactic sugar
///   Defined at compile time — no runtime reflection cost
///
/// RULES:
///   1. Must be in a static class
///   2. Method must be static
///   3. First parameter has 'this' keyword — that is the extended type
///   4. Class must be at namespace level (not nested)
///   5. Resolved at compile time — cannot override existing instance methods
///      If an instance method and extension method have the same signature,
///      the INSTANCE METHOD always wins
///
/// HOW LINQ IS BUILT:
///   Every LINQ method (Where, Select, OrderBy, GroupBy etc.) is an
///   extension method on IEnumerable<T> defined in System.Linq.Enumerable
///   That is the most famous use of extension methods in .NET
///
/// INTERVIEW CRITICAL:
///   Extension methods cannot access private members
///   They cannot be called on null without explicitly checking
///   They cannot override virtual methods
///   They are syntactic sugar — compiled to static method calls
///   IEnumerable<T>.Where() IS an extension method
///
/// ═══════════════════════════════════════════════════════════════════
/// </summary>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Refresher.ExtensionMethods
{
    public class ExtMethods 
    {
        public static void Exec()
        {
            Console.WriteLine("Extension Methods — Reference\n");

            DemonstrateStringExtensions();
            DemonstrateNumericExtensions();
            DemonstrateCollectionExtensions();
            DemonstrateNullSafety();
            DemonstrateFluentBuilder();
            DemonstrateLinqIsExtensions();
            DemonstrateInterfaceExtensions();
            DemonstrateChaining();
        }

        // ═══════════════════════════════════════════════════════════════
        // STRING EXTENSIONS
        // ═══════════════════════════════════════════════════════════════
        static void DemonstrateStringExtensions()
        {
            Console.WriteLine("── STRING EXTENSIONS ──────────────────────────────────");

            string loanId = "  loan-2024-tx-00123  ";

            // Called exactly like instance methods
            Console.WriteLine(loanId.ToLoanId());          // LOAN-2024-TX-00123
            Console.WriteLine(loanId.IsValidLoanId());     // True
            Console.WriteLine("".IsNullOrWhiteSpace());    // True
            Console.WriteLine("hello".Repeat(3));          // hellohellohello
            Console.WriteLine("hello world".ToPascalCase()); // HelloWorld
            Console.WriteLine("HelloWorld".ToSnakeCase()); // hello_world
            Console.WriteLine("350000".ToDecimalOrDefault()); // 350000
            Console.WriteLine("abc".ToDecimalOrDefault(-1));   // -1
            Console.WriteLine("Loan Application".Truncate(8)); // Loan App...
            Console.WriteLine("hello".Capitalize());           // Hello

            // Calling on a variable vs literal — same syntax
            string s = "test";
            Console.WriteLine(s.Repeat(2));       // testtest
            Console.WriteLine("x".Repeat(2));     // xx
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // NUMERIC EXTENSIONS
        // ═══════════════════════════════════════════════════════════════
        static void DemonstrateNumericExtensions()
        {
            Console.WriteLine("── NUMERIC EXTENSIONS ─────────────────────────────────");

            decimal amount = 350_000.755m;

            Console.WriteLine(amount.RoundToNearest(0.25m));   // 350000.75
            Console.WriteLine(amount.ToFormattedCurrency());    // $350,000.76
            Console.WriteLine(350_000m.IsBetween(100_000m, 500_000m)); // True
            Console.WriteLine((-50m).IsNegative());             // True
            Console.WriteLine(6.875.ToPercent());               // 6.875%

            int years = 30;
            Console.WriteLine(years.ToMonths());                // 360
            Console.WriteLine(5.IsEven());                      // False
            Console.WriteLine(4.IsEven());                      // True

            // Fluent numeric pipeline
            decimal result = 350_000m
                .ApplyRate(0.06875m)
                .RoundToNearest(0.01m);
            Console.WriteLine($"Applied rate: {result:F2}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // COLLECTION EXTENSIONS
        // ═══════════════════════════════════════════════════════════════
        static void DemonstrateCollectionExtensions()
        {
            Console.WriteLine("── COLLECTION EXTENSIONS ──────────────────────────────");

            var loanIds = new List<string>
            {
                "LOAN-001", "LOAN-002", null, "LOAN-003", null, "LOAN-004"
            };

            // WhereNotNull — common pattern, not in standard LINQ
            var validIds = loanIds.WhereNotNull().ToList();
            Console.WriteLine($"WhereNotNull: {validIds.Count} items"); // 4

            // IsNullOrEmpty
            Console.WriteLine($"IsNullOrEmpty: {loanIds.IsNullOrEmpty()}"); // False
            Console.WriteLine($"IsNullOrEmpty: {((List<string>)null).IsNullOrEmpty()}"); // True

            // Batch — split into chunks
            var numbers = Enumerable.Range(1, 10).ToList();
            foreach (var batch in numbers.Batch(3))
                Console.WriteLine($"Batch: [{string.Join(",", batch)}]");

            // ForEach — LINQ doesn't have one on IEnumerable
            var amounts = new[] { 100_000m, 200_000m, 350_000m };
            amounts.ForEach(a => Console.Write($"{a:C} "));
            Console.WriteLine();

            // DistinctBy — pre .NET 6 pattern
            var loans = new[]
            {
                new { Id = "L1", Status = "PENDING" },
                new { Id = "L2", Status = "APPROVED" },
                new { Id = "L3", Status = "PENDING" }
            };
            var distinctStatuses = loans.DistinctByKey(l => l.Status).ToList();
            Console.WriteLine($"DistinctBy Status: {distinctStatuses.Count}"); // 2

            // ToDelimitedString
            var ids = new[] { "LOAN-001", "LOAN-002", "LOAN-003" };
            Console.WriteLine(ids.ToDelimitedString(", ")); // LOAN-001, LOAN-002, LOAN-003
            Console.WriteLine(ids.ToDelimitedString(" | ")); // LOAN-001 | LOAN-002 | LOAN-003

            // Shuffle
            var shuffled = numbers.Shuffle().Take(5).ToList();
            Console.WriteLine($"Shuffled sample: [{string.Join(",", shuffled)}]");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // NULL SAFETY WITH EXTENSIONS
        // ═══════════════════════════════════════════════════════════════
        //
        // INTERVIEW CRITICAL:
        //   Extension methods CAN be called on null — the 'this' parameter
        //   receives null like any other parameter. The method must handle it.
        //   Instance methods throw NullReferenceException on null.
        //   Extension methods on null are valid — they compile and run.
        //
        static void DemonstrateNullSafety()
        {
            Console.WriteLine("── NULL SAFETY ─────────────────────────────────────────");

            string nullStr = null;

            // These WOULD throw NullReferenceException:
            // nullStr.ToUpper()    — instance method on null = exception
            // nullStr.Length       — instance method on null = exception

            // These work — extension methods handle null explicitly:
            Console.WriteLine(nullStr.IsNullOrWhiteSpace()); // True — no exception
            Console.WriteLine(nullStr.ToLoanId());           // (empty) — handled
            Console.WriteLine(nullStr.ToDecimalOrDefault(0)); // 0 — handled

            // The compiled form of extension method reveals why:
            // nullStr.IsNullOrWhiteSpace()
            // compiles to:
            // StringExtensions.IsNullOrWhiteSpace(nullStr)
            // Which is just a static call — null is valid as a parameter

            List<string> nullList = null;
            Console.WriteLine(nullList.IsNullOrEmpty()); // True — no exception

            Console.WriteLine("Extension methods on null: safe when designed for it");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // FLUENT BUILDER PATTERN WITH EXTENSIONS
        // ═══════════════════════════════════════════════════════════════
        //
        // Extension methods enable fluent APIs on types you don't own
        // Each method returns the same type enabling chaining
        // This is how StringBuilder fluent API works internally
        //
        static void DemonstrateFluentBuilder()
        {
            Console.WriteLine("── FLUENT BUILDER ─────────────────────────────────────");

            // StringBuilder already has fluent chaining built in
            // But you can add your own domain-specific extensions:
            string auditLog = new StringBuilder()
                .AppendLoanEntry("LOAN-001", "SUBMITTED", "Alice")
                .AppendLoanEntry("LOAN-001", "CREDIT_CHECK", "System")
                .AppendLoanEntry("LOAN-001", "APPROVED", "Underwriter")
                .ToString();

            Console.WriteLine(auditLog);

            // Fluent validation chain
            string loanId = "LOAN-2024-001";
            bool isValid = loanId
                .IsNotNullOrEmpty()
                .And(loanId.StartsWith("LOAN-"))
                .And(loanId.Length > 5);

            Console.WriteLine($"Fluent validation: {isValid}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // LINQ IS EXTENSION METHODS
        // ═══════════════════════════════════════════════════════════════
        //
        // Every LINQ method is an extension method on IEnumerable<T>
        // Defined in System.Linq namespace in static class Enumerable
        // This is the most famous real-world use of extension methods
        //
        static void DemonstrateLinqIsExtensions()
        {
            Console.WriteLine("── LINQ = EXTENSION METHODS ───────────────────────────");

            var loans = new[]
            {
                new LoanRecord("L1", 350_000m, "APPROVED"),
                new LoanRecord("L2", 150_000m, "PENDING"),
                new LoanRecord("L3", 500_000m, "APPROVED"),
                new LoanRecord("L4",  75_000m, "DENIED"),
                new LoanRecord("L5", 425_000m, "PENDING"),
            };

            // All of these are extension methods on IEnumerable<T>
            // Where, Select, OrderBy, GroupBy, Sum, Count, First, Any, All
            // are ALL defined as:
            // public static IEnumerable<T> Where<T>(this IEnumerable<T> source, ...)

            var approved = loans
                .Where(l => l.Status == "APPROVED")      // extension method
                .OrderByDescending(l => l.Amount)         // extension method
                .Select(l => $"{l.Id}: {l.Amount:C}")    // extension method
                .ToList();                                // extension method

            approved.ForEach(Console.WriteLine);

            decimal totalApproved = loans
                .Where(l => l.Status == "APPROVED")
                .Sum(l => l.Amount);                      // extension method
            Console.WriteLine($"Total approved: {totalApproved:C}");

            var byStatus = loans
                .GroupBy(l => l.Status)                   // extension method
                .Select(g => $"{g.Key}: {g.Count()} loans");

            foreach (var g in byStatus)
                Console.WriteLine(g);

            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // EXTENDING INTERFACES
        // ═══════════════════════════════════════════════════════════════
        //
        // Extending an interface extends ALL implementing classes
        // This is how LINQ extends IEnumerable<T> —
        // List<T>, Array, HashSet<T>, all get Where() for free
        // because they all implement IEnumerable<T>
        //
        static void DemonstrateInterfaceExtensions()
        {
            Console.WriteLine("── INTERFACE EXTENSIONS ───────────────────────────────");

            // ILoanProcessor — extending an interface
            ILoanProcessor standard = new StandardProcessor();
            ILoanProcessor express = new ExpressProcessor();

            // ProcessAndLog defined as extension on ILoanProcessor
            // Both implementations get it for free
            standard.ProcessAndLog("LOAN-001");
            express.ProcessAndLog("LOAN-002");

            // Extending IComparable<T> — works on any comparable type
            Console.WriteLine($"\n5.Clamp(1,10):  {5.Clamp(1, 10)}");   // 5
            Console.WriteLine($"15.Clamp(1,10): {15.Clamp(1, 10)}");   // 10
            Console.WriteLine($"(-5).Clamp(1,10): {(-5).Clamp(1, 10)}"); // 1
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // METHOD CHAINING — THE REAL POWER
        // ═══════════════════════════════════════════════════════════════
        static void DemonstrateChaining()
        {
            Console.WriteLine("── METHOD CHAINING ────────────────────────────────────");

            // Without extensions — nested static calls, inside-out reading
            string result1 = StringExtensions.Capitalize(
                StringExtensions.ToLoanId(
                    "  loan-2024-tx-001  "));

            // With extensions — left-to-right, readable pipeline
            string result2 = "  loan-2024-tx-001  "
                .ToLoanId()
                .Capitalize();

            Console.WriteLine($"Chained: {result2}");

            // Real pipeline — loan processing
            var rawInput = "  loan-2024-001  ";
            bool canProcess = rawInput
                .ToLoanId()
                .IsValidLoanId()
                .And(rawInput.Trim().Length > 0);

            Console.WriteLine($"Can process: {canProcess}");
            Console.WriteLine();

            // WHAT THE COMPILER ACTUALLY DOES:
            // "hello".Repeat(3)
            // compiles to:
            // StringExtensions.Repeat("hello", 3)
            // The dot-call syntax is pure compile-time transformation
            // Zero runtime overhead vs calling the static method directly
            Console.WriteLine("Compiler transforms: x.Method(args)");
            Console.WriteLine("               into: ClassName.Method(x, args)");
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // EXTENSION METHOD CLASSES
    // RULE: static class, static methods, 'this' on first param
    // ═══════════════════════════════════════════════════════════════════

    public static class StringExtensions
    {
        // Basic null-safe check
        public static bool IsNullOrWhiteSpace(this string s)
            => string.IsNullOrWhiteSpace(s);

        public static bool IsNotNullOrEmpty(this string s)
            => !string.IsNullOrEmpty(s);

        // Domain-specific — format as loan ID
        public static string ToLoanId(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return string.Empty;
            return s.Trim().ToUpperInvariant();
        }

        // Validation
        public static bool IsValidLoanId(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return false;
            var trimmed = s.Trim().ToUpperInvariant();
            return trimmed.StartsWith("LOAN-") && trimmed.Length > 5;
        }

        // Repeat — not in standard library
        public static string Repeat(this string s, int count)
        {
            if (string.IsNullOrEmpty(s) || count <= 0) return string.Empty;
            return new StringBuilder(s.Length * count)
                .Insert(0, s, count)
                .ToString();
        }

        // Truncate with ellipsis
        public static string Truncate(this string s, int maxLength, string suffix = "...")
        {
            if (string.IsNullOrEmpty(s) || s.Length <= maxLength) return s;
            return s.Substring(0, maxLength) + suffix;
        }

        // Capitalize first letter
        public static string Capitalize(this string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return char.ToUpper(s[0]) + s.Substring(1);
        }

        // PascalCase from space-separated words
        public static string ToPascalCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            return string.Join("",
                s.Split(' ')
                 .Where(w => !string.IsNullOrEmpty(w))
                 .Select(w => char.ToUpper(w[0]) + w.Substring(1).ToLower()));
        }

        // snake_case from PascalCase
        public static string ToSnakeCase(this string s)
        {
            if (string.IsNullOrWhiteSpace(s)) return s;
            var sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                if (char.IsUpper(s[i]) && i > 0) sb.Append('_');
                sb.Append(char.ToLower(s[i]));
            }
            return sb.ToString();
        }

        // Safe parse
        public static decimal ToDecimalOrDefault(this string s, decimal defaultValue = 0m)
            => decimal.TryParse(s, out decimal result) ? result : defaultValue;

        // Fluent boolean composition
        public static bool And(this bool left, bool right) => left && right;
        public static bool Or(this bool left, bool right) => left || right;
    }

    public static class NumericExtensions
    {
        // Decimal extensions
        public static decimal RoundToNearest(this decimal value, decimal step)
        {
            if (step == 0) return value;
            return Math.Round(value / step) * step;
        }

        public static string ToFormattedCurrency(this decimal value)
            => value.ToString("C");

        public static bool IsBetween(this decimal value, decimal min, decimal max)
            => value >= min && value <= max;

        public static bool IsNegative(this decimal value) => value < 0;

        public static decimal ApplyRate(this decimal principal, decimal annualRate)
            => principal * annualRate;

        // Double extensions
        public static string ToPercent(this double value)
            => $"{value:F3}%";

        // Int extensions
        public static int ToMonths(this int years) => years * 12;

        public static bool IsEven(this int n) => n % 2 == 0;
        public static bool IsOdd(this int n) => n % 2 != 0;

        // Generic clamp — works on any IComparable<T>
        public static T Clamp<T>(this T value, T min, T max)
            where T : IComparable<T>
        {
            if (value.CompareTo(min) < 0) return min;
            if (value.CompareTo(max) > 0) return max;
            return value;
        }
    }

    public static class CollectionExtensions
    {
        // WhereNotNull — extremely common pattern
        public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T> source)
            where T : class
        {
            if (source == null) yield break;
            foreach (var item in source)
                if (item != null) yield return item;
        }

        // Null-safe IsNullOrEmpty — works on null collections
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
            => source == null || !source.Any();

        // Batch — split into chunks of size n
        public static IEnumerable<IEnumerable<T>> Batch<T>(
            this IEnumerable<T> source, int size)
        {
            var batch = new List<T>(size);
            foreach (var item in source)
            {
                batch.Add(item);
                if (batch.Count == size)
                {
                    yield return batch;
                    batch = new List<T>(size);
                }
            }
            if (batch.Count > 0) yield return batch;
        }

        // ForEach on IEnumerable — missing from standard LINQ
        public static void ForEach<T>(this IEnumerable<T> source, Action<T> action)
        {
            if (source == null) return;
            foreach (var item in source) action(item);
        }

        // DistinctBy — available in .NET 6+ natively
        public static IEnumerable<T> DistinctByKey<T, TKey>(
            this IEnumerable<T> source, Func<T, TKey> keySelector)
        {
            var seen = new HashSet<TKey>();
            foreach (var item in source)
                if (seen.Add(keySelector(item)))
                    yield return item;
        }

        // ToDelimitedString
        public static string ToDelimitedString<T>(
            this IEnumerable<T> source, string delimiter = ",")
            => string.Join(delimiter, source);

        // Shuffle — Fisher-Yates
        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source)
        {
            var list = source.ToList();
            var rng = new Random();
            for (int i = list.Count - 1; i > 0; i--)
            {
                int j = rng.Next(i + 1);
                (list[i], list[j]) = (list[j], list[i]);
            }
            return list;
        }
    }

    public static class StringBuilderExtensions
    {
        // Domain-specific extension on StringBuilder
        public static StringBuilder AppendLoanEntry(
            this StringBuilder sb, string loanId, string action, string actor)
        {
            sb.AppendLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}] {loanId} | {action} | {actor}");
            return sb; // Return this for chaining
        }
    }

    // EXTENDING AN INTERFACE — all implementors get the extension
    public interface ILoanProcessor
    {
        string ProcessorName { get; }
        void Process(string loanId);
    }

    public static class LoanProcessorExtensions
    {
        // All ILoanProcessor implementations get this for free
        public static void ProcessAndLog(this ILoanProcessor processor, string loanId)
        {
            Console.WriteLine($"[{processor.ProcessorName}] Starting: {loanId}");
            processor.Process(loanId);
            Console.WriteLine($"[{processor.ProcessorName}] Complete: {loanId}");
        }
    }

    public class StandardProcessor : ILoanProcessor
    {
        public string ProcessorName => "Standard";
        public void Process(string loanId) =>
            Console.WriteLine($"  Processing {loanId} via standard queue");
    }

    public class ExpressProcessor : ILoanProcessor
    {
        public string ProcessorName => "Express";
        public void Process(string loanId) =>
            Console.WriteLine($"  Processing {loanId} via priority queue");
    }

    // SUPPORTING TYPES
    public record LoanRecord(string Id, decimal Amount, string Status);
}