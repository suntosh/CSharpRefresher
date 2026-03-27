/// <summary>
/// Nullable Patterns — Interview Reference
/// ═══════════════════════════════════════════════════════════════════
///
/// TWO DISTINCT NULLABLE SYSTEMS IN C#:
///
/// 1. NULLABLE VALUE TYPES (C# 2.0+)
///    Nullable<T> or T?
///    Allows value types (int, decimal, bool, struct) to hold null
///    Has .HasValue, .Value, .GetValueOrDefault() properties
///    int?  decimal?  bool?  DateTime?  Guid?
///
/// 2. NULLABLE REFERENCE TYPES (C# 8.0+)
///    Compile-time annotation system — NOT runtime
///    string  = non-nullable (compiler warns if null assigned)
///    string? = explicitly nullable reference
///    Enable with: #nullable enable
///    Does NOT prevent null at runtime — only compiler warnings
///
/// NULL OPERATORS — KNOW ALL FIVE:
///   ?.   null conditional      — short-circuit to null
///   ??   null coalescing       — fallback value
///   ??=  null coalescing assign — assign only if null
///   !    null forgiving        — suppress nullable warning
///   is null / is not null      — pattern matching null check
///
/// INTERVIEW CRITICAL:
///   null == null  → true
///   null != null  → false
///   null + ""     → "" (string concatenation treats null as "")
///   (string)null  → null (safe cast)
///   null.Length   → NullReferenceException
///   NaN != NaN    → true (only value not equal to itself)
///   Nullable<T>.Value when HasValue=false → InvalidOperationException
///
/// ═══════════════════════════════════════════════════════════════════

#nullable enable

using System;
using System.Collections.Generic;

namespace Refresher
{
    public class NullablePatterns
    {
        public static void Exec()
        {
            Console.WriteLine("Nullable Patterns — Interview Reference\n");

            DemonstrateNullableValueTypes();
            DemonstrateNullOperators();
            DemonstrateNullableReferenceTypes();
            DemonstratePatternMatching();
            DemonstrateNullGuardPatterns();
            DemonstrateCommonNullMistakes();
            DemonstrateNullInCollections();
        }

        // ═══════════════════════════════════════════════════════════════
        // NULLABLE VALUE TYPES — Nullable<T> / T?
        // ═══════════════════════════════════════════════════════════════
        //
        // Value types (int, decimal, bool, struct, enum) cannot be null
        // by default. Nullable<T> wraps them in a struct with a flag.
        //
        // INTERNAL STRUCTURE of Nullable<T>:
        //   struct Nullable<T> where T : struct {
        //     private bool hasValue;
        //     private T value;
        //   }
        //
        // Size = sizeof(T) + 1 byte for the bool flag (+ padding)
        //   int?     = 8 bytes (4 for int + 4 for alignment)
        //   decimal? = 20 bytes (16 for decimal + 4 for alignment)
        //
        // BOXING behavior:
        //   Nullable<T> with value boxes as T (not as Nullable<T>)
        //   Nullable<T> without value (null) boxes as null reference
        //   This is special CLR behavior for Nullable<T>
        //
        // THREE-STATE LOGIC with bool?:
        //   true & null  = null
        //   false & null = false (short-circuit)
        //   true | null  = true  (short-circuit)
        //   false | null = null
        //
        static void DemonstrateNullableValueTypes()
        {
            Console.WriteLine("── NULLABLE VALUE TYPES ───────────────────────────────");

            // Declaration
            int? nullableInt = null;
            decimal? nullableDecimal = 350_000.00m;
            bool? nullableBool = null;
            DateTime? nullableDate = null;
            Guid? nullableGuid = Guid.NewGuid();

            // HasValue and Value properties
            Console.WriteLine($"nullableInt.HasValue:     {nullableInt.HasValue}");     // False
            Console.WriteLine($"nullableDecimal.HasValue: {nullableDecimal.HasValue}"); // True
            Console.WriteLine($"nullableDecimal.Value:    {nullableDecimal.Value:C}");  // $350,000.00

            // DANGER: .Value on null throws InvalidOperationException
            try
            {
                int danger = nullableInt!.Value; // ! suppresses warning — still throws at runtime
            }
            catch (InvalidOperationException ex)
            {
                Console.WriteLine($"\nValue on null: {ex.GetType().Name}");
            }

            // Safe access patterns
            int safe1 = nullableInt.GetValueOrDefault();       // 0 — type default
            int safe2 = nullableInt.GetValueOrDefault(-1);     // -1 — custom default
            int safe3 = nullableInt ?? -1;                     // -1 — null coalescing
            Console.WriteLine($"\nGetValueOrDefault():   {safe1}");
            Console.WriteLine($"GetValueOrDefault(-1): {safe2}");
            Console.WriteLine($"?? -1:                 {safe3}");

            // Implicit conversion from T to T?
            int x = 42;
            int? y = x;   // Implicit — always safe
            Console.WriteLine($"\nImplicit T→T?: {y}");

            // Explicit conversion from T? to T — can throw if null
            int? z = 42;
            int w = (int)z;  // Explicit cast — throws if null
            Console.WriteLine($"Explicit T?→T: {w}");

            // Three-state bool logic
            bool? t = true;
            bool? f = false;
            bool? n = null;

            Console.WriteLine($"\nThree-state bool:");
            Console.WriteLine($"true  & null  = {t & n}");  // null
            Console.WriteLine($"false & null  = {f & n}");  // false (short-circuit)
            Console.WriteLine($"true  | null  = {t | n}");  // true  (short-circuit)
            Console.WriteLine($"false | null  = {f | n}");  // null

            // Nullable in switch expression
            string loanStatus = nullableDecimal switch
            {
                null => "No amount specified",
                < 100_000m => "Small loan",
                < 500_000m => "Standard loan",
                _ => "Jumbo loan"
            };
            Console.WriteLine($"\nLoan status: {loanStatus}");

            // Boxing behavior — special CLR treatment
            object boxedNull = nullableInt;          // null — not boxed as Nullable<int>
            object boxedValue = nullableDecimal;      // boxed as decimal, not Nullable<decimal>
            Console.WriteLine($"\nBoxed null nullable: {boxedNull == null}");  // True
            Console.WriteLine($"Boxed value type:    {boxedValue?.GetType().Name}"); // Decimal
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // NULL OPERATORS — ALL FIVE
        // ═══════════════════════════════════════════════════════════════
        //
        // MASTER THESE — they appear in every C# interview
        //
        static void DemonstrateNullOperators()
        {
            Console.WriteLine("── NULL OPERATORS ─────────────────────────────────────");

            // ─────────────────────────────────────────────────────────
            // 1. ?. NULL CONDITIONAL OPERATOR
            // ─────────────────────────────────────────────────────────
            // Short-circuits to null if left side is null
            // No NullReferenceException — returns null instead
            // Can chain multiple levels
            // Result type is always nullable (T?)
            //
            Console.WriteLine("1. ?. Null Conditional:");

            string? name = null;
            int? length = name?.Length;              // null — not NullReferenceException
            string? upper = name?.ToUpper();         // null
            Console.WriteLine($"  null?.Length:   {length}");  // (blank — null)
            Console.WriteLine($"  null?.ToUpper:  {upper}");   // (blank — null)

            // Chaining — each ?. checks for null before proceeding
            LoanApplication? loan = null;
            string? city = loan?.Borrower?.Address?.City;  // null — no exception
            Console.WriteLine($"  Deep chain null: {city}");

            loan = new LoanApplication
            {
                Borrower = new Borrower { Name = "Alice", Address = null }
            };
            city = loan?.Borrower?.Address?.City; // null — Address is null
            Console.WriteLine($"  Chain with null Address: {city}");

            // ?. with method calls
            List<string>? list = null;
            int? count = list?.Count;  // null
            list?.Add("item");          // No-op if null — does not throw
            Console.WriteLine($"  null list count: {count}");

            // ?[] null conditional indexer
            string[]? arr = null;
            string? first = arr?[0];  // null — not IndexOutOfRangeException
            Console.WriteLine($"  null array index: {first}");

            // ─────────────────────────────────────────────────────────
            // 2. ?? NULL COALESCING OPERATOR
            // ─────────────────────────────────────────────────────────
            // Returns left if not null, otherwise right
            // Right side is only evaluated if left is null (lazy)
            // Can chain: a ?? b ?? c ?? d
            //
            Console.WriteLine("\n2. ?? Null Coalescing:");

            string? customerName = null;
            string displayName = customerName ?? "Anonymous";
            Console.WriteLine($"  null ?? 'Anonymous': {displayName}");

            // Chaining
            string? first1 = null, second = null, third = "Found";
            string result = first1 ?? second ?? third ?? "Default";
            Console.WriteLine($"  Chain: {result}"); // "Found"

            // With nullable value types
            int? score = null;
            int finalScore = score ?? 0;
            Console.WriteLine($"  int? ?? 0: {finalScore}"); // 0

            // ?? vs conditional: identical result, ?? is cleaner
            string a1 = customerName != null ? customerName : "Anonymous";
            string a2 = customerName ?? "Anonymous";
            _ = a1; _ = a2; // suppress unused warnings — both produce same result

            // ─────────────────────────────────────────────────────────
            // 3. ??= NULL COALESCING ASSIGNMENT (C# 8+)
            // ─────────────────────────────────────────────────────────
            // Assigns right to left ONLY if left is null
            // Left must be a variable, property, or indexer
            //
            Console.WriteLine("\n3. ??= Null Coalescing Assignment:");

            string? config = null;
            config ??= "default-config";  // Assigns because config is null
            Console.WriteLine($"  After ??=: {config}"); // "default-config"

            config ??= "other-config";    // Does NOT assign — config is not null
            Console.WriteLine($"  After second ??=: {config}"); // Still "default-config"

            // Lazy initialization pattern
            List<string>? _cache = null;
            _cache ??= new List<string>(); // Initialize only if null
            _cache.Add("item");
            Console.WriteLine($"  Lazy init count: {_cache.Count}");

            // ─────────────────────────────────────────────────────────
            // 4. ! NULL FORGIVING OPERATOR (C# 8+)
            // ─────────────────────────────────────────────────────────
            // Suppresses nullable compiler warning
            // DOES NOT prevent NullReferenceException at runtime
            // Use only when YOU know the value is not null but compiler can't prove it
            // Overuse is a code smell — prefer proper null checks
            //
            Console.WriteLine("\n4. ! Null Forgiving:");

            string? possiblyNull = GetMaybeNull();
            // Compiler warns: possiblyNull might be null
            // If you KNOW it won't be null in this context:
            int len = possiblyNull!.Length; // Suppresses warning — crashes if actually null
            Console.WriteLine($"  Forgiving access: {len}");

            // Better pattern — use ?? or check first
            int safelen = possiblyNull?.Length ?? 0;
            Console.WriteLine($"  Safe access: {safelen}");

            // ─────────────────────────────────────────────────────────
            // 5. is null / is not null PATTERN MATCHING
            // ─────────────────────────────────────────────────────────
            // Preferred over == null in nullable reference type contexts
            // == null can be overloaded by custom types
            // is null always checks for actual null — cannot be overridden
            //
            Console.WriteLine("\n5. is null / is not null:");

            string? val = null;
            if (val is null)
                Console.WriteLine("  val is null: true");

            val = "hello";
            if (val is not null)
                Console.WriteLine($"  val is not null: {val}");

            // is null in expressions
            bool isNull = val is null;
            bool isNotNull = val is not null;
            Console.WriteLine($"  is null: {isNull}, is not null: {isNotNull}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // NULLABLE REFERENCE TYPES — C# 8+ COMPILE-TIME SYSTEM
        // ═══════════════════════════════════════════════════════════════
        //
        // #nullable enable — activates the nullable reference type system
        // This is a COMPILE-TIME feature only. No runtime change.
        // Annotations tell the compiler your intent — it warns on violations.
        //
        // WITHOUT nullable enable:
        //   string s = null;  // No warning — as it always was
        //
        // WITH nullable enable:
        //   string  s = null;  // Warning: null assigned to non-nullable
        //   string? s = null;  // OK — explicitly nullable
        //
        // ANNOTATIONS:
        //   string    — non-nullable, should never be null
        //   string?   — nullable, may be null
        //   T         — depends on context
        //   T?        — nullable (for reference types: T? same as T but annotated)
        //
        // FLOW ANALYSIS:
        //   Compiler tracks whether a nullable variable has been null-checked
        //   After null check, treats it as non-nullable in that branch
        //
        // WHY THIS MATTERS FOR INTERVIEWS:
        //   .NET team is making nullable reference types the default
        //   New projects should have #nullable enable
        //   Understanding the annotation system is Principal-level knowledge
        //
        static void DemonstrateNullableReferenceTypes()
        {
            Console.WriteLine("── NULLABLE REFERENCE TYPES (C# 8+) ──────────────────");

            // Non-nullable — compiler warns if you try to assign null
            string nonNullable = "hello";
            _ = nonNullable;

            // Nullable — explicitly declared as potentially null
            string? nullable = null;
            nullable = "world";
            _ = nullable;

            // Flow analysis — compiler tracks null state
            string? maybeNull = GetMaybeNull();

            // Compiler warns: maybeNull may be null
            // int len = maybeNull.Length;          // Warning: CS8602 dereference

            // After null check — compiler knows it's safe
            if (maybeNull != null)
            {
                int len = maybeNull.Length;         // No warning — null checked above
                Console.WriteLine($"Safe length: {len}");
            }

            // Pattern matching null check — also satisfies flow analysis
            if (maybeNull is not null)
            {
                Console.WriteLine($"Pattern safe: {maybeNull.Length}");
            }

            // Null coalescing also works
            int safeLen = maybeNull?.Length ?? 0;  // Always safe
            Console.WriteLine($"Coalescing length: {safeLen}");

            // Method annotations
            string result = NonNullReturn();    // Annotated as non-null return
            string? nullResult = NullableReturn(); // Annotated as nullable return

            Console.WriteLine($"\nNonNull return: {result}");
            Console.WriteLine($"Nullable return: {nullResult ?? "(null)"}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // NULL PATTERN MATCHING
        // ═══════════════════════════════════════════════════════════════
        //
        // C# 7+ pattern matching integrates with null checking
        // More expressive than traditional null checks
        //
        static void DemonstratePatternMatching()
        {
            Console.WriteLine("── NULL PATTERN MATCHING ──────────────────────────────");

            object? obj = null;

            // is null check
            Console.WriteLine($"obj is null: {obj is null}");       // True
            Console.WriteLine($"obj is not null: {obj is not null}"); // False

            obj = "hello";
            Console.WriteLine($"\nAfter assignment:");
            Console.WriteLine($"obj is null: {obj is null}"); // False
            if (obj is string strVal)
                Console.WriteLine($"obj is string, value: {strVal}"); // "hello"

            // Switch expression with null
            string? status = null;
            string display = status switch
            {
                null => "Not specified",
                "PENDING" => "Under Review",
                "APPROVED" => "✓ Approved",
                "DENIED" => "✗ Denied",
                string s2 when s2.StartsWith("APPEAL") => $"Appeal: {s2}",
                _ => status
            };
            Console.WriteLine($"\nNull in switch: {display}"); // "Not specified"

            status = "APPROVED";
            display = status switch
            {
                null => "Not specified",
                "APPROVED" => "✓ Approved",
                _ => status
            };
            Console.WriteLine($"Approved switch: {display}");

            // Nested null pattern
            LoanApplication? app = new LoanApplication
            {
                Borrower = new Borrower { Name = "Bob", Address = null },
                Amount = 275_000m
            };

            string addressDisplay = app switch
            {
                null => "No application",
                { Borrower: null } => "No borrower",
                { Borrower.Address: null } => "No address on file",
                { Borrower.Address.City: var city } => $"City: {city}"
            };
            Console.WriteLine($"\nNested pattern: {addressDisplay}"); // "No address on file"
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // NULL GUARD PATTERNS — PRODUCTION PATTERNS
        // ═══════════════════════════════════════════════════════════════
        //
        // Different contexts require different null guard strategies
        //
        static void DemonstrateNullGuardPatterns()
        {
            Console.WriteLine("── NULL GUARD PATTERNS ────────────────────────────────");

            // Pattern 1: ArgumentNullException.ThrowIfNull (C# 10+)
            // Best for public API parameter validation
            static decimal ProcessLoan(string loanId, decimal amount)
            {
                ArgumentNullException.ThrowIfNull(loanId);        // C# 10+
                ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
                return amount * 1.06875m;
            }

            Console.WriteLine($"ProcessLoan: {ProcessLoan("LOAN-001", 100m):F2}");

            // Pattern 2: Guard clause — early return
            static string? FormatLoanId(string? raw)
            {
                if (string.IsNullOrWhiteSpace(raw)) return null; // Guard clause
                return raw.Trim().ToUpper();
            }

            Console.WriteLine($"FormatLoanId(null): {FormatLoanId(null) ?? "(null)"}");
            Console.WriteLine($"FormatLoanId('  loan-1 '): {FormatLoanId("  loan-1 ")}");

            // Pattern 3: Null object pattern — return empty/default instead of null
            static IEnumerable<string> GetLoanIds(string? customerId)
            {
                if (customerId is null)
                    return Array.Empty<string>(); // Never return null from collections
                return new[] { "LOAN-001", "LOAN-002" };
            }

            var ids = GetLoanIds(null);
            Console.WriteLine($"\nNull object pattern count: {System.Linq.Enumerable.Count(ids)}"); // 0 — not null

            // Pattern 4: Result type pattern (manual Either monad)
            static (decimal? Amount, string? Error) TryCalculatePayment(decimal? principal)
            {
                if (principal is null) return (null, "Principal is required");
                if (principal <= 0) return (null, "Principal must be positive");
                return (principal * 0.06875m / 12, null);
            }

            var (amount, error) = TryCalculatePayment(null);
            Console.WriteLine($"\nResult null: amount={amount}, error={error}");

            var (amount2, error2) = TryCalculatePayment(350_000m);
            Console.WriteLine($"Result ok: amount={amount2:F2}, error={error2}");

            // Pattern 5: ?? chain for configuration fallback
            string? envConfig = null; // Environment.GetEnvironmentVariable("DB_HOST")
            string? fileConfig = null; // From config file
            string defaultConfig = "localhost";

            string dbHost = envConfig ?? fileConfig ?? defaultConfig;
            Console.WriteLine($"\nConfig chain: {dbHost}"); // "localhost"
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // COMMON NULL MISTAKES — INTERVIEW TRAPS
        // ═══════════════════════════════════════════════════════════════
        static void DemonstrateCommonNullMistakes()
        {
            Console.WriteLine("── COMMON NULL MISTAKES ───────────────────────────────");

            // Mistake 1: null + string concatenation
            string? s = null;
            string result = s + "world"; // null treated as "" — no exception
            Console.WriteLine($"null + 'world' = '{result}'"); // "world"

            string result2 = "hello" + s; // Also fine
            Console.WriteLine($"'hello' + null = '{result2}'"); // "hello"

            // Mistake 2: string.Format with null
            string formatted = string.Format("Name: {0}", (object?)null);
            Console.WriteLine($"Format null: '{formatted}'"); // "Name: "

            // Mistake 3: Comparing nullable value to null incorrectly
            int? n = null;
            Console.WriteLine($"\nn == null:    {n == null}");    // True
            Console.WriteLine($"n is null:    {n is null}");    // True — preferred
            Console.WriteLine($"!n.HasValue:  {!n.HasValue}");  // True — equivalent

            // Mistake 4: Default of nullable is null, not 0
            int? defaultNullable = default; // null — NOT 0
            int defaultInt = default; // 0
            Console.WriteLine($"\ndefault(int?): {defaultNullable}"); // (null)
            Console.WriteLine($"default(int):  {defaultInt}");       // 0

            // Mistake 5: Nullable in LINQ
            var numbers = new int?[] { 1, null, 3, null, 5 };

            // Sum ignores nulls
            decimal sum = 0;
            foreach (var num in numbers)
                sum += num ?? 0;
            Console.WriteLine($"\nSum with null skipping: {sum}"); // 9

            // Mistake 6: == with custom types that override ==
            // is null is safer — cannot be overloaded
            // Always use is null for null checks when overloading is possible

            // Mistake 7: Null propagation with side effects
            List<string>? items = null;
            items?.Add("item"); // Does NOT add — returns null, Add never called
            Console.WriteLine($"\nNull list after ?.Add: {items}"); // Still null

            // Mistake 8: Forgetting null in switch
            string? grade = null;
            try
            {
                string letter = grade switch
                {
                    "A" => "Excellent",
                    "B" => "Good",
                    // null case missing — default arm calls grade!.ToUpper() which throws
                    _ => grade!.ToUpper() // NullReferenceException if grade is null
                };
                _ = letter;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Switch null miss: {ex.GetType().Name}");
            }
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // NULL IN COLLECTIONS
        // ═══════════════════════════════════════════════════════════════
        static void DemonstrateNullInCollections()
        {
            Console.WriteLine("── NULL IN COLLECTIONS ────────────────────────────────");

            // List can contain null reference type values
            var list = new List<string?> { "Alice", null, "Bob", null };
            Console.WriteLine($"List with nulls count: {list.Count}"); // 4

            // Filter nulls — LINQ WhereNotNull pattern
            var nonNulls = new List<string>();
            foreach (var item in list)
                if (item is not null) nonNulls.Add(item);
            Console.WriteLine($"Non-null count: {nonNulls.Count}"); // 2

            // Dictionary null behavior
            var dict = new Dictionary<string, string?>();
            dict["key1"] = "value";
            dict["key2"] = null;         // Value can be null

            Console.WriteLine($"\nDict null value: {dict["key2"] ?? "(null)"}");

            // Dictionary key cannot be null — throws ArgumentNullException
            try
            {
                dict[null!] = "value"; // Throws
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Dict null key: ArgumentNullException");
            }

            // ContainsKey vs TryGetValue
            bool hasKey = dict.ContainsKey("key2");        // True
            bool hasVal = dict.TryGetValue("key2", out string? val);  // True, val=null
            Console.WriteLine($"\nContainsKey: {hasKey}, TryGetValue: {hasVal}, val: {val}");

            // RULE: Never return null from methods that return collections
            // Return empty collection instead — caller doesn't need null check
            static List<string> GetItems(bool empty)
            {
                if (empty) return new List<string>(); // NOT null
                return new List<string> { "item1", "item2" };
            }

            var result = GetItems(true);
            Console.WriteLine($"\nEmpty collection (not null): count={result.Count}");

            // Summary
            Console.WriteLine("\n── NULL OPERATOR SUMMARY ──────────────────────────────");
            Console.WriteLine($"{"Operator",-8} {"Name",-30} {"Use When"}");
            Console.WriteLine(new string('─', 70));
            Console.WriteLine($"{"?.",-8} {"Null Conditional",-30} {"Safe member access on nullable"}");
            Console.WriteLine($"{"??",-8} {"Null Coalescing",-30} {"Provide fallback for null"}");
            Console.WriteLine($"{"??=",-8} {"Null Coalescing Assign",-30} {"Lazy initialize if null"}");
            Console.WriteLine($"{"!",-8} {"Null Forgiving",-30} {"Suppress warning (use sparingly)"}");
            Console.WriteLine($"{"is null",-8} {"Null Pattern",-30} {"Preferred null check — unoverridable"}");
        }

        // ═══════════════════════════════════════════════════════════════
        // HELPER METHODS
        // ═══════════════════════════════════════════════════════════════

        static string? GetMaybeNull() => "hello"; // Annotated as nullable return

        static string NonNullReturn() => "guaranteed non-null";

        static string? NullableReturn() => null;
    }

    // ═══════════════════════════════════════════════════════════════════
    // SUPPORTING TYPES
    // ═══════════════════════════════════════════════════════════════════

    public class LoanApplication
    {
        public string? LoanId { get; set; }
        public decimal? Amount { get; set; }
        public Borrower? Borrower { get; set; }
        public DateTime? SubmittedAt { get; set; }
    }

    public class Borrower
    {
        public string? Name { get; set; }
        public Address? Address { get; set; }
    }

    public class Address
    {
        public string? Street { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Zip { get; set; }
    }
}