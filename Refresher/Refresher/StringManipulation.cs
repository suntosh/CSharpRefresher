/// <summary>
/// String Manipulation — Interview Reference
/// ═══════════════════════════════════════════════════════════════════
///
/// CORE FACTS:
///   - string is IMMUTABLE — every modification creates a new object
///   - string is a REFERENCE TYPE but behaves like a value type
///   - string == string does VALUE comparison (overrides ==)
///   - string interning — identical literals share the same object
///   - Default value: null (not empty string)
///   - string.Empty is preferred over "" — same object, clearer intent
///
/// PERFORMANCE HIERARCHY (fastest to slowest for concatenation):
///   1. Span<char> / stackalloc    — zero heap allocation
///   2. string.Create              — single allocation, known length
///   3. StringBuilder              — amortized O(1) append
///   4. string.Concat / Join       — single allocation for known inputs
///   5. String interpolation $""   — optimized in .NET 6+ via handler
///   6. string + string in loop    — O(n²) allocations — NEVER do this
///
/// ═══════════════════════════════════════════════════════════════════
/// </summary>

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace Refresher
{
    class StringOps
    {
        static void Exec()
        {
            Console.WriteLine("String Manipulation — Interview Reference\n");

            DemonstrateImmutability();
            DemonstrateComparison();
            DemonstrateStringBuilder();
            DemonstrateSpan();
            DemonstrateCommonOps();
            DemonstrateFormatting();
            DemonstrateSearchAndSplit();
            DemonstrateHotPathPatterns();
        }

        // ═══════════════════════════════════════════════════════════════
        // IMMUTABILITY — THE FOUNDATION
        // ═══════════════════════════════════════════════════════════════
        //
        // Every string "modification" creates a NEW string on the heap.
        // The original is unchanged. GC must collect the old one.
        //
        // This is why:
        //   s += "x"  in a loop = O(n²) allocations
        //   string.ToUpper() returns NEW string, does not modify s
        //   string.Replace() returns NEW string
        //
        // INTERVIEW: Why is string immutable?
        //   1. Thread safety — immutable objects are inherently thread-safe
        //   2. String interning — safe to share references to same literal
        //   3. Hash code stability — Dictionary<string,T> keys are safe
        //   4. Security — prevents tampering with shared string references
        //
        static void DemonstrateImmutability()
        {
            Console.WriteLine("── IMMUTABILITY ───────────────────────────────────────");

            string s = "hello";
            string upper = s.ToUpper(); // NEW string — s is unchanged

            Console.WriteLine($"original: {s}");     // "hello"
            Console.WriteLine($"upper:    {upper}"); // "HELLO"

            // Concatenation in loop — O(n²) — NEVER DO THIS
            string bad = "";
            for (int i = 0; i < 5; i++)
                bad += i; // Creates new string every iteration
            Console.WriteLine($"bad concat result: {bad}"); // "01234"

            // String interning — identical literals share same reference
            string a = "hello";
            string b = "hello";
            Console.WriteLine($"\nReferenceEqual literals: {ReferenceEquals(a, b)}"); // True — interned

            string c = new string('h', 1) + "ello"; // Runtime constructed
            Console.WriteLine($"ReferenceEqual runtime:  {ReferenceEquals(a, c)}"); // False — not interned
            Console.WriteLine($"Value equal:             {a == c}");                 // True
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // STRING COMPARISON — INTERVIEW CRITICAL
        // ═══════════════════════════════════════════════════════════════
        //
        // ORDINAL vs LINGUISTIC comparison:
        //   Ordinal   — byte-by-byte comparison of UTF-16 code units
        //               Fast, predictable, culture-independent
        //               Use for: file paths, URLs, identifiers, keys
        //
        //   Linguistic — culture-aware, handles accents, case folding
        //               Slower, locale-dependent
        //               Use for: user-visible text, sorting for display
        //
        // StringComparison enum values:
        //   Ordinal                  — case-sensitive, no culture
        //   OrdinalIgnoreCase        — case-insensitive, no culture ← most common
        //   CurrentCulture           — case-sensitive, current locale
        //   CurrentCultureIgnoreCase — case-insensitive, current locale
        //   InvariantCulture         — case-sensitive, invariant locale
        //   InvariantCultureIgnoreCase — case-insensitive, invariant
        //
        // RULE: Always specify StringComparison explicitly
        //   Avoids locale-dependent bugs in production
        //   Turkish locale: "i".ToUpper() = "İ" (dotted capital I)
        //   Ordinal avoids this entire class of bugs
        //
        static void DemonstrateComparison()
        {
            Console.WriteLine("── COMPARISON ─────────────────────────────────────────");

            string s1 = "Hello";
            string s2 = "hello";

            // == uses Ordinal comparison for string
            Console.WriteLine($"== : {s1 == s2}");  // False

            // Always specify StringComparison
            bool equalOrdinal = string.Equals(s1, s2, StringComparison.Ordinal);
            bool equalIgnore = string.Equals(s1, s2, StringComparison.OrdinalIgnoreCase);

            Console.WriteLine($"Ordinal:           {equalOrdinal}"); // False
            Console.WriteLine($"OrdinalIgnoreCase: {equalIgnore}");  // True

            // CompareTo for sorting
            int result = string.Compare("apple", "banana", StringComparison.Ordinal);
            Console.WriteLine($"\n'apple' vs 'banana': {result}"); // negative — apple < banana

            // StartsWith / EndsWith — always use StringComparison
            string url = "https://api.example.com";
            bool isHttps = url.StartsWith("https", StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"IsHttps: {isHttps}"); // True

            // Contains — case-insensitive search
            string text = "Loan Application Processing";
            bool found = text.Contains("application", StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"Contains 'application': {found}"); // True
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // STRINGBUILDER — THE MUTABLE STRING
        // ═══════════════════════════════════════════════════════════════
        //
        // StringBuilder is a MUTABLE buffer — appends in-place.
        // Single allocation (or amortized O(1) if capacity exceeded).
        // Use when: building strings in loops or from many parts.
        //
        // INTERNALS:
        //   Internally a char[] buffer with a length tracker
        //   When buffer fills, doubles capacity (like List<T>)
        //   ToString() creates a single string from the buffer
        //
        // CAPACITY OPTIMIZATION:
        //   new StringBuilder(capacity) — pre-allocate if size known
        //   Avoids internal resizing entirely
        //   For 1000-char output: new StringBuilder(1024)
        //
        // WHEN NOT TO USE StringBuilder:
        //   Fewer than 3-4 concatenations — string.Concat is faster
        //   Known fixed number of parts — string.Format or interpolation
        //   StringBuilder has overhead too — object allocation + buffer
        //
        // CHAINING — StringBuilder returns itself from Append:
        //   sb.Append("a").Append("b").AppendLine("c")
        //
        static void DemonstrateStringBuilder()
        {
            Console.WriteLine("── STRINGBUILDER ──────────────────────────────────────");

            // Basic usage
            var sb = new StringBuilder();
            sb.Append("Loan");
            sb.Append(" Application");
            sb.AppendLine(" #12345");         // Appends + newline
            sb.AppendFormat("Amount: {0:C}", 350_000m);
            string result = sb.ToString();
            Console.WriteLine(result);

            // Pre-sized — avoids internal resizing
            var sb2 = new StringBuilder(256); // Expected output ~200 chars
            for (int i = 0; i < 10; i++)
                sb2.AppendFormat("Item {0}: {1}\n", i, i * 100);
            Console.WriteLine($"Pre-sized result length: {sb2.Length}");

            // Fluent chaining
            string csv = new StringBuilder()
                .Append("CustomerId,Name,Amount")
                .AppendLine()
                .Append("1,Alice,350000")
                .AppendLine()
                .Append("2,Bob,275000")
                .ToString();
            Console.WriteLine($"\nCSV:\n{csv}");

            // StringBuilder manipulation
            var sb3 = new StringBuilder("Hello World");
            sb3.Replace("World", "Santosh"); // In-place replace
            sb3.Insert(5, ",");              // Insert at index
            sb3.Remove(0, 6);               // Remove chars
            Console.WriteLine($"Manipulated: {sb3}");

            // Capacity management
            var sb4 = new StringBuilder(16);
            Console.WriteLine($"\nCapacity: {sb4.Capacity}"); // 16
            sb4.Append(new string('x', 20));                  // Exceeds capacity
            Console.WriteLine($"After overflow capacity: {sb4.Capacity}"); // Auto-doubled
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // SPAN<T> AND MEMORY<T> — ZERO ALLOCATION STRING OPS
        // ═══════════════════════════════════════════════════════════════
        //
        // Span<char> — a view over a contiguous region of memory
        //   No heap allocation — stack-allocated struct
        //   Cannot be stored in a class field (ref struct restriction)
        //   Cannot be used with async/await (stack only)
        //   Perfect for parsing, slicing, and hot path string operations
        //
        // ReadOnlySpan<char> — immutable view, accepted by string methods
        //   string.AsSpan() — converts string to ReadOnlySpan<char>
        //   Slice operations are O(1) — just move pointer + length
        //   No substring allocation
        //
        // Memory<T> — heap-based span, can be stored in fields
        //   Use when you need to pass slices across async boundaries
        //   or store them in class fields
        //
        // stackalloc — allocate on the stack (no GC pressure)
        //   Use for small, known-size buffers
        //   Span<char> buf = stackalloc char[64];
        //
        // INTERVIEW: Why use Span<T>?
        //   Substring() allocates a new string — O(n) allocation
        //   AsSpan().Slice() is O(1) — no allocation, just a view
        //   Critical in high-throughput APIs (HTTP parsing, JSON, etc.)
        //
        static void DemonstrateSpan()
        {
            Console.WriteLine("── SPAN<T> — ZERO ALLOCATION ──────────────────────────");

            string loanNumber = "LOAN-2024-TX-00123456";

            // Traditional — allocates new strings
            string year1 = loanNumber.Substring(5, 4); // heap allocation

            // Span — zero allocation view
            ReadOnlySpan<char> span = loanNumber.AsSpan();
            ReadOnlySpan<char> year2 = span.Slice(5, 4); // NO allocation

            Console.WriteLine($"Traditional substring: {year1}");
            Console.WriteLine($"Span slice:            {year2.ToString()}");

            // Parsing without allocation
            ReadOnlySpan<char> numberPart = span.Slice(span.LastIndexOf('-') + 1);
            if (int.TryParse(numberPart, out int loanId))
                Console.WriteLine($"Parsed loan ID: {loanId}"); // 123456

            // stackalloc — stack allocation for small buffers
            Span<char> buffer = stackalloc char[64];
            "hello world".AsSpan().CopyTo(buffer);
            // Uppercase in-place — no heap allocation
            for (int i = 0; i < 11; i++)
                buffer[i] = char.ToUpper(buffer[i]);
            Console.WriteLine($"stackalloc upper: {buffer.Slice(0, 11).ToString()}");

            // string.Create — single allocation with known length
            string formatted = string.Create(10, 12345678m, (chars, amount) =>
            {
                // Write directly into the string's buffer
                amount.ToString("F2").AsSpan().CopyTo(chars);
            });
            Console.WriteLine($"string.Create: {formatted}");

            // ArrayPool for large buffers — avoid LOH pressure
            char[] rented = ArrayPool<char>.Shared.Rent(1024);
            try
            {
                Span<char> pooled = rented.AsSpan(0, 1024);
                // Use pooled buffer — no heap allocation beyond the pool
                "loan data".AsSpan().CopyTo(pooled);
                Console.WriteLine($"ArrayPool first chars: {pooled.Slice(0, 9).ToString()}");
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rented); // ALWAYS return
            }
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // COMMON STRING OPERATIONS
        // ═══════════════════════════════════════════════════════════════
        //
        // All return NEW strings — original is never modified
        //
        static void DemonstrateCommonOps()
        {
            Console.WriteLine("── COMMON OPERATIONS ──────────────────────────────────");

            string s = "  Hello, World!  ";

            // Trimming
            Console.WriteLine($"Trim:      '{s.Trim()}'");
            Console.WriteLine($"TrimStart: '{s.TrimStart()}'");
            Console.WriteLine($"TrimEnd:   '{s.TrimEnd()}'");

            // Case
            string clean = s.Trim();
            Console.WriteLine($"\nToUpper: {clean.ToUpper()}");
            Console.WriteLine($"ToLower: {clean.ToLower()}");

            // Search
            Console.WriteLine($"\nIndexOf 'World':     {clean.IndexOf("World")}");    // 7
            Console.WriteLine($"LastIndexOf 'l':     {clean.LastIndexOf('l')}");      // 10
            Console.WriteLine($"Contains 'World':    {clean.Contains("World")}");     // True
            Console.WriteLine($"StartsWith 'Hello':  {clean.StartsWith("Hello")}");   // True
            Console.WriteLine($"EndsWith '!':        {clean.EndsWith('!')}");         // True

            // Modification — always returns new string
            Console.WriteLine($"\nReplace: {clean.Replace("World", "Santosh")}");
            Console.WriteLine($"Remove:  {clean.Remove(7, 6)}");      // Remove "World,"
            Console.WriteLine($"Insert:  {clean.Insert(5, " Dear")}"); // Insert after Hello

            // Substring
            Console.WriteLine($"\nSubstring(7):    {clean.Substring(7)}");       // "World!"
            Console.WriteLine($"Substring(7,5):  {clean.Substring(7, 5)}");     // "World"

            // Padding
            string amount = "350000";
            Console.WriteLine($"\nPadLeft:  {amount.PadLeft(10, '0')}");   // "0000350000"
            Console.WriteLine($"PadRight: {amount.PadRight(10, ' ')}|");   // "350000    |"

            // Length
            Console.WriteLine($"\nLength: {clean.Length}");
            Console.WriteLine($"[0]:    {clean[0]}");   // Indexer — O(1)
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // STRING FORMATTING
        // ═══════════════════════════════════════════════════════════════
        //
        // Four approaches — know when to use each:
        //
        // 1. string.Format — classic, boxing for value types pre-.NET 6
        // 2. $"" interpolation — syntactic sugar, optimized in .NET 6+
        //    .NET 6+: uses DefaultInterpolatedStringHandler — fewer allocs
        //    Pre-.NET 6: compiles to string.Format — boxes value types
        // 3. StringBuilder.AppendFormat — for loop/multi-part building
        // 4. string.Create — zero allocation when length is known
        //
        // FORMAT SPECIFIERS — know these cold:
        //   C or c  — currency:    350000 → $350,000.00
        //   D or d  — decimal int: 42 → 42 (D5 → 00042)
        //   E or e  — scientific:  3.5e+5
        //   F or f  — fixed point: 3.14159 (F2 → 3.14)
        //   G or g  — general:     shorter of E or F
        //   N or n  — number:      1234567 → 1,234,567.00
        //   P or p  — percent:     0.0675 → 6.75%
        //   R or r  — round-trip:  preserves all digits
        //   X or x  — hex:         255 → FF
        //
        static void DemonstrateFormatting()
        {
            Console.WriteLine("── FORMATTING ─────────────────────────────────────────");

            decimal amount = 350_000.50m;
            double rate = 0.06875;
            int loanId = 12345;
            DateTime date = new DateTime(2024, 3, 15);

            // Currency
            Console.WriteLine($"Currency:   {amount:C}");       // $350,000.50
            Console.WriteLine($"Currency2:  {amount:C2}");      // $350,000.50

            // Number
            Console.WriteLine($"Number:     {amount:N}");       // 350,000.50
            Console.WriteLine($"Number0:    {amount:N0}");      // 350,001

            // Fixed point
            Console.WriteLine($"Fixed2:     {rate:F4}");        // 0.0688
            Console.WriteLine($"Percent:    {rate:P2}");        // 6.88%

            // Integer padding
            Console.WriteLine($"Decimal5:   {loanId:D8}");      // 00012345
            Console.WriteLine($"Hex:        {loanId:X}");       // 3039

            // Date formatting
            Console.WriteLine($"\nDate short: {date:d}");       // 3/15/2024
            Console.WriteLine($"Date long:  {date:D}");        // Friday, March 15, 2024
            Console.WriteLine($"Date ISO:   {date:yyyy-MM-dd}"); // 2024-03-15
            Console.WriteLine($"Date time:  {date:yyyy-MM-dd HH:mm:ss}");

            // String.Format — classic approach
            string formatted = string.Format(
                "Loan {0:D8} — Amount: {1:C} — Rate: {2:P2}",
                loanId, amount, rate);
            Console.WriteLine($"\nFormatted: {formatted}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // SEARCH AND SPLIT
        // ═══════════════════════════════════════════════════════════════
        static void DemonstrateSearchAndSplit()
        {
            Console.WriteLine("── SEARCH AND SPLIT ───────────────────────────────────");

            string csv = "Alice,350000,APPROVED,2024-03-15";

            // Split — always allocates string array
            string[] parts = csv.Split(',');
            Console.WriteLine($"Parts[0]: {parts[0]}"); // Alice
            Console.WriteLine($"Parts[1]: {parts[1]}"); // 350000

            // Split with limit
            string[] limited = csv.Split(',', 2); // Only split on first comma
            Console.WriteLine($"Limited[1]: {limited[1]}"); // "350000,APPROVED,2024-03-15"

            // Join — inverse of split
            string[] names = { "Alice", "Bob", "Carol" };
            string joined = string.Join(", ", names);
            Console.WriteLine($"\nJoined: {joined}"); // "Alice, Bob, Carol"

            // String.Concat — most efficient for fixed known parts
            string concat = string.Concat("Loan", "-", "2024", "-", "TX");
            Console.WriteLine($"Concat: {concat}");

            // IndexOf patterns
            string text = "Loan Amount: $350,000.00";
            int colonIdx = text.IndexOf(':');
            string valueStr = text.Substring(colonIdx + 2); // "$350,000.00"
            Console.WriteLine($"\nExtracted: {valueStr}");

            // Span-based split — zero allocation parsing
            ReadOnlySpan<char> csvSpan = csv.AsSpan();
            int commaIdx = csvSpan.IndexOf(',');
            ReadOnlySpan<char> nameSpan = csvSpan.Slice(0, commaIdx);
            Console.WriteLine($"Span name: {nameSpan.ToString()}"); // No allocation
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // HOT PATH PATTERNS — PRODUCTION CRITICAL
        // ═══════════════════════════════════════════════════════════════
        //
        // These patterns matter in high-throughput systems:
        //   - HTTP request parsing
        //   - CSV/JSON processing
        //   - Log formatting
        //   - Financial data processing
        //
        // RULES FOR HOT PATHS:
        //   1. Never use string + string in loops
        //   2. Pre-size StringBuilder when output length is predictable
        //   3. Use Span<char> for read-only slicing operations
        //   4. Use ArrayPool for large temporary char buffers
        //   5. Use string.IsNullOrWhiteSpace over null checks + empty checks
        //   6. Cache .Length in a local if accessed in a tight loop
        //   7. Use char overloads where available — IndexOf(char) > IndexOf(string)
        //
        static void DemonstrateHotPathPatterns()
        {
            Console.WriteLine("── HOT PATH PATTERNS ──────────────────────────────────");

            // Pattern 1: Null/empty checks — prefer IsNullOrWhiteSpace
            string input = "   ";
            Console.WriteLine($"IsNullOrEmpty:      {string.IsNullOrEmpty(input)}");     // False — whitespace
            Console.WriteLine($"IsNullOrWhiteSpace: {string.IsNullOrWhiteSpace(input)}"); // True

            // Pattern 2: char overload is faster than string overload
            string loanNum = "LOAN-2024-TX-00123";
            int idx1 = loanNum.IndexOf('-');          // char overload — faster
            int idx2 = loanNum.IndexOf("-");           // string overload — slower
            Console.WriteLine($"\nchar indexOf: {idx1}, string indexOf: {idx2}"); // Both 4

            // Pattern 3: Avoid repeated Substring — use Span
            // Bad — allocates new string
            string year_bad = loanNum.Substring(5, 4);

            // Good — zero allocation
            ReadOnlySpan<char> year_good = loanNum.AsSpan(5, 4);
            Console.WriteLine($"Year (Span): {year_good.ToString()}");

            // Pattern 4: String.Concat for small fixed joins — faster than $""
            string a = "Hello", b = " ", c = "World";
            string joined = string.Concat(a, b, c);     // Single allocation
            string interp = $"{a}{b}{c}";               // Same in .NET 6+
            Console.WriteLine($"\nConcat: {joined}");

            // Pattern 5: Pre-sized StringBuilder for audit log generation
            var auditEntries = new[] {
                ("LOGIN", "user123", "2024-03-15 09:00"),
                ("APPLY", "user123", "2024-03-15 09:01"),
                ("SUBMIT", "user123", "2024-03-15 09:05")
            };
            int estimatedSize = auditEntries.Length * 60;
            var sb = new StringBuilder(estimatedSize);
            foreach (var (action, user, time) in auditEntries)
                sb.AppendLine($"[{time}] {user}: {action}");
            Console.WriteLine($"\nAudit log:\n{sb}");

            // Pattern 6: String.Create for known-length formatted strings
            static string FormatLoanId(int id) =>
                string.Create(12, id, (chars, value) =>
                {
                    chars[0] = 'L'; chars[1] = 'N'; chars[2] = '-';
                    value.ToString("D8").AsSpan().CopyTo(chars.Slice(3));
                    chars[11] = 'X';
                });

            Console.WriteLine($"Formatted ID: {FormatLoanId(12345)}");
            Console.WriteLine();

            // SUMMARY TABLE
            Console.WriteLine("── PERFORMANCE SUMMARY ────────────────────────────────");
            Console.WriteLine($"{"Approach",-30} {"Allocations",-15} {"Use When"}");
            Console.WriteLine(new string('─', 70));
            Console.WriteLine($"{"Span<char> / stackalloc",-30} {"Zero",-15} {"Hot path parsing"}");
            Console.WriteLine($"{"string.Create",-30} {"One",-15} {"Known output length"}");
            Console.WriteLine($"{"StringBuilder (pre-sized)",-30} {"One",-15} {"Loop building"}");
            Console.WriteLine($"{"StringBuilder (default)",-30} {"Amortized",-15} {"Unknown length loop"}");
            Console.WriteLine($"{"string.Concat(a,b,c)",-30} {"One",-15} {"Few fixed parts"}");
            Console.WriteLine($"{"$\"\" interpolation .NET6+",-30} {"One",-15} {"General use"}");
            Console.WriteLine($"{"string + string in loop",-30} {"O(n²)",-15} {"NEVER"}");
        }
    }
}