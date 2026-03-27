/// <summary>
/// C# Data Types — Complete Interview Reference
/// ═══════════════════════════════════════════════════════════════════
///
/// TWO CATEGORIES:
///   Value Types  — stored on stack, copied on assignment
///   Reference Types — stored on heap, passed by reference
///
/// QUICK REFERENCE TABLE:
///
/// Type        Category   Size      Range                           Default  Suffix
/// ─────────────────────────────────────────────────────────────────────────────────
/// bool        Value      1 byte    true / false                    false    —
/// byte        Value      1 byte    0 to 255                        0        —
/// sbyte       Value      1 byte    -128 to 127                     0        —
/// short       Value      2 bytes   -32,768 to 32,767               0        —
/// ushort      Value      2 bytes   0 to 65,535                     0        —
/// int         Value      4 bytes   -2.1B to 2.1B                   0        —
/// uint        Value      4 bytes   0 to 4.29B                      0        u/U
/// long        Value      8 bytes   -9.2Q to 9.2Q                   0L       l/L
/// ulong       Value      8 bytes   0 to 18.4Q                      0UL      ul/UL
/// float       Value      4 bytes   ±1.5e-45 to ±3.4e38 (7 digits)  0f       f/F
/// double      Value      8 bytes   ±5e-324 to ±1.7e308 (15 digits) 0d       d/D
/// decimal     Value      16 bytes  ±1e-28 to ±7.9e28 (28 digits)   0m       m/M
/// char        Value      2 bytes   U+0000 to U+FFFF (Unicode)      '\0'     —
/// nint        Value      4/8 bytes Platform-dependent (ptr size)   0        —
/// nuint       Value      4/8 bytes Platform-dependent (ptr size)   0        —
///
/// string      Reference  Variable  Immutable Unicode sequence       null     —
/// object      Reference  Variable  Base of all types                null     —
/// dynamic     Reference  Variable  Resolved at runtime              null     —
///
/// B  = Billion, Q = Quintillion
/// ═══════════════════════════════════════════════════════════════════
/// </summary>

using System;
using System.Text;

namespace Refresher
{
    class DataTypes
    {
        public static void Exec()
        {
            Console.WriteLine("C# Data Types — Interview Reference");
            Console.WriteLine("====================================\n");

            DemonstrateValueTypes();
            DemonstrateIntegerTypes();
            DemonstrateFloatingPointTypes();
            DemonstrateDecimal();
            DemonstrateChar();
            DemonstrateString();
            DemonstrateNullable();
            DemonstrateTypeConversion();
            DemonstrateBoxingUnboxing();
            DemonstrateVarAndDynamic();
            DemonstrateSpecialTypes();
        }

        // ═══════════════════════════════════════════════════════════════
        // VALUE TYPES vs REFERENCE TYPES
        // ═══════════════════════════════════════════════════════════════
        //
        // VALUE TYPES:
        //   - Stored directly on the stack (or inline in containing object)
        //   - Copied on assignment — two independent copies
        //   - All numeric types, bool, char, struct, enum
        //   - Cannot be null (unless Nullable<T>)
        //   - Default value is zero/false/'\0' — never uninitialized
        //
        // REFERENCE TYPES:
        //   - Variable holds a reference (pointer) to heap memory
        //   - Assignment copies the reference — two variables, one object
        //   - string, object, class, interface, array, delegate
        //   - Can be null (reference points to nothing)
        //   - Default value is null
        //
        // INTERVIEW TRAP: string is a reference type but behaves like
        // a value type because it is IMMUTABLE. Every "modification"
        // creates a new string object. This is why string == string
        // works by value comparison despite being a reference type.
        //
        static void DemonstrateValueTypes()
        {
            Console.WriteLine("── VALUE TYPE COPY BEHAVIOR ──────────────────────────");

            int a = 10;
            int b = a;   // b gets a COPY of a's value
            b = 20;      // modifying b does NOT affect a

            Console.WriteLine($"a = {a}, b = {b}");  // a=10, b=20 — independent

            // Reference type — shared reference
            int[] arr1 = { 1, 2, 3 };
            int[] arr2 = arr1;   // arr2 points to SAME array
            arr2[0] = 99;        // modifying arr2 DOES affect arr1

            Console.WriteLine($"arr1[0] = {arr1[0]}");  // 99 — shared!
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // INTEGER TYPES
        // ═══════════════════════════════════════════════════════════════
        //
        // SIGNED integers (positive and negative):
        //   sbyte  — 1 byte,  -128 to 127
        //   short  — 2 bytes, -32,768 to 32,767
        //   int    — 4 bytes, -2,147,483,648 to 2,147,483,647  ← DEFAULT
        //   long   — 8 bytes, -9,223,372,036,854,775,808 to 9,223,372,036,854,775,807
        //
        // UNSIGNED integers (non-negative only):
        //   byte   — 1 byte,  0 to 255                ← INTERVIEW: only unsigned byte
        //   ushort — 2 bytes, 0 to 65,535
        //   uint   — 4 bytes, 0 to 4,294,967,295      ← NOT "unsigned int"
        //   ulong  — 8 bytes, 0 to 18,446,744,073,709,551,615
        //
        // CRITICAL INTERVIEW FACTS:
        //   - Default integer type is int (32-bit signed)
        //   - Use long for values > 2.1 billion (phone numbers, Unix timestamps)
        //   - Use uint/ulong for bit manipulation, hash values, file sizes
        //   - Integer overflow wraps silently by default — use checked{} to throw
        //   - int.MaxValue + 1 = int.MinValue (wraps to negative)
        //
        // NATIVE INTEGERS (C# 9+):
        //   nint  — signed, size matches platform pointer (4 bytes x86, 8 bytes x64)
        //   nuint — unsigned, same platform-dependent size
        //   Used for: interop with native code, unsafe pointer arithmetic
        //
        static void DemonstrateIntegerTypes()
        {
            Console.WriteLine("── INTEGER TYPES ─────────────────────────────────────");

            // Signed
            sbyte sb2 = -128;
            ushort us2 = 65_535;
            uint ui2 = 4_294_967_295U;
            ulong ul2 = 18_446_744_073_709_551_615UL;
            _ = sb2; _ = us2; _ = ui2; _ = ul2;

            Console.WriteLine($"sbyte  min: {sbyte.MinValue},  max: {sbyte.MaxValue}");
            Console.WriteLine($"short  min: {short.MinValue},  max: {short.MaxValue}");
            Console.WriteLine($"int    min: {int.MinValue},  max: {int.MaxValue}");
            Console.WriteLine($"long   min: {long.MinValue},  max: {long.MaxValue}");
            Console.WriteLine($"byte   min: {byte.MinValue},   max: {byte.MaxValue}");
            Console.WriteLine($"ushort min: {ushort.MinValue},  max: {ushort.MaxValue}");
            Console.WriteLine($"uint   min: {uint.MinValue},  max: {uint.MaxValue}");
            Console.WriteLine($"ulong  min: {ulong.MinValue},  max: {ulong.MaxValue}");

            // Overflow behavior — INTERVIEW CRITICAL
            int maxInt = int.MaxValue;
            int overflow = maxInt + 1;  // Silently wraps to int.MinValue
            Console.WriteLine($"\nint.MaxValue + 1 = {overflow}");  // -2147483648

            // Prevent silent overflow with checked
            try
            {
                int safe = checked(maxInt + 1); // Throws OverflowException
                _ = safe;
            }
            catch (OverflowException)
            {
                Console.WriteLine("checked{} correctly threw OverflowException");
            }

            // Numeric literal formats
            int hex = 0xFF;        // Hexadecimal — 255
            int binary = 0b1111_1111; // Binary — 255
            int readable = 1_000_000;  // Underscore separator — 1000000
            Console.WriteLine($"\nhex: {hex}, binary: {binary}, readable: {readable}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // FLOATING POINT TYPES — float and double
        // ═══════════════════════════════════════════════════════════════
        //
        // float  — 32-bit IEEE 754, ~7 significant digits,  suffix: f or F
        // double — 64-bit IEEE 754, ~15 significant digits, suffix: d or D (optional)
        //
        // KEY FACT: double is the DEFAULT for floating point literals
        //   3.14    → double (no suffix needed)
        //   3.14f   → float  (f suffix required)
        //   3.14d   → double (d suffix optional)
        //
        // CRITICAL INTERVIEW FACT — BINARY FLOATING POINT IMPRECISION:
        //   float and double use BINARY representation internally
        //   0.1 cannot be represented exactly in binary (like 1/3 in decimal)
        //   This causes rounding errors that accumulate in calculations
        //
        //   0.1 + 0.2 == 0.3  → FALSE in floating point
        //   Use decimal for financial calculations — NEVER float or double
        //   Use double for scientific/engineering calculations where precision
        //   is bounded and small errors are acceptable
        //
        // Special values:
        //   double.NaN            — Not a Number (0.0/0.0)
        //   double.PositiveInfinity — 1.0/0.0
        //   double.NegativeInfinity — -1.0/0.0
        //   double.Epsilon        — smallest positive double value
        //
        static void DemonstrateFloatingPointTypes()
        {
            Console.WriteLine("── FLOATING POINT — float and double ─────────────────");

            float f = 3.14f;
            double d = 3.14;
            _ = f; _ = d;

            // The infamous floating point imprecision
            double result = 0.1 + 0.2;
            Console.WriteLine($"0.1 + 0.2 = {result}");           // 0.30000000000000004
            Console.WriteLine($"0.1 + 0.2 == 0.3: {result == 0.3}"); // False!

            // Correct comparison for floating point
            double epsilon = 1e-10;
            bool almostEqual = Math.Abs(result - 0.3) < epsilon;
            Console.WriteLine($"Within epsilon: {almostEqual}");   // True

            // Special values
            double nan = double.NaN;
            double posInf = double.PositiveInfinity;
            double negInf = double.NegativeInfinity;

            Console.WriteLine($"\ndouble.NaN: {nan}");
            Console.WriteLine($"NaN == NaN: {nan == nan}");        // FALSE! NaN is never equal to itself
            Console.WriteLine($"double.IsNaN(nan): {double.IsNaN(nan)}"); // Use IsNaN()
            Console.WriteLine($"1.0/0.0: {1.0 / 0.0}");           // Infinity, not exception
            Console.WriteLine($"0.0/0.0: {0.0 / 0.0}");           // NaN, not exception
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // DECIMAL — THE FINANCIAL TYPE
        // ═══════════════════════════════════════════════════════════════
        //
        // decimal — 128-bit (16 bytes), base-10 floating point
        //           28-29 significant decimal digits
        //           Range: ±1.0 × 10⁻²⁸ to ±7.9 × 10²⁸
        //           Suffix: m or M (REQUIRED for literals)
        //
        // WHY decimal FOR FINANCE — INTERVIEW CRITICAL:
        //   Unlike float/double which use binary floating point,
        //   decimal uses BASE-10 representation internally.
        //   This means 0.1m is stored EXACTLY as 0.1, not as an
        //   approximation. Financial calculations don't accumulate
        //   rounding errors.
        //
        //   Mortgage rate 6.875% stored as float: 6.87499... (wrong)
        //   Mortgage rate 6.875% stored as decimal: 6.875 (exact)
        //
        // TRADEOFFS vs double:
        //   decimal is SLOWER than double — software emulated, not hardware
        //   decimal has SMALLER range than double
        //   decimal has MORE precision than double for base-10 numbers
        //
        // WHEN TO USE WHAT:
        //   decimal → money, financial calculations, anything base-10 exact
        //   double  → scientific/engineering, performance-critical math,
        //             trigonometry, square roots (Math library uses double)
        //   float   → graphics (GPU), large arrays where memory matters
        //
        static void DemonstrateDecimal()
        {
            Console.WriteLine("── DECIMAL — FINANCIAL PRECISION ─────────────────────");

            decimal d = 3.14159265358979323846m; // m suffix REQUIRED
            decimal loanAmount = 350_000.00m;
            decimal interestRate = 6.875m;       // Exact — no binary approximation
            decimal monthlyRate = interestRate / 100m / 12m;

            Console.WriteLine($"Loan amount:   {loanAmount:C}");
            Console.WriteLine($"Interest rate: {interestRate}%");
            Console.WriteLine($"Monthly rate:  {monthlyRate:F8}");

            // Proof of precision
            decimal decResult = 0.1m + 0.2m;
            Console.WriteLine($"\n0.1m + 0.2m = {decResult}");           // 0.3 exactly
            Console.WriteLine($"0.1m + 0.2m == 0.3m: {decResult == 0.3m}"); // True!

            // decimal does NOT have NaN or Infinity
            // Division by zero throws DivideByZeroException
            // Use a variable — compiler rejects constant decimal division by zero
            try
            {
                decimal zero = 0m;
                decimal bad = 1m / zero; // Runtime DivideByZeroException
            }
            catch (DivideByZeroException)
            {
                Console.WriteLine("decimal division by zero throws exception (unlike double)");
            }

            // decimal min/max
            Console.WriteLine($"\ndecimal.MaxValue: {decimal.MaxValue}");
            Console.WriteLine($"decimal.MinValue: {decimal.MinValue}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // CHAR
        // ═══════════════════════════════════════════════════════════════
        //
        // char — 16-bit Unicode character (UTF-16 code unit)
        //        Range: U+0000 to U+FFFF
        //        Size: 2 bytes
        //        Default: '\0' (null character)
        //        Literal: single quotes 'A'
        //
        // KEY FACTS:
        //   char is a VALUE TYPE (unlike Java where char is primitive but similar)
        //   char can be implicitly converted to int (its Unicode code point)
        //   char arithmetic is valid — 'A' + 1 = 'B'
        //   char represents a UTF-16 code unit — some Unicode chars need 2 chars (surrogate pairs)
        //
        // INTERVIEW: difference between char and string
        //   char  — single character, value type, single quotes
        //   string — sequence of chars, reference type, double quotes, immutable
        //
        static void DemonstrateChar()
        {
            Console.WriteLine("── CHAR ───────────────────────────────────────────────");

            char c = 'A';
            char newline = '\n';
            char tab = '\t';
            char unicode = '\u0041';
            char nullChar = '\0';
            _ = newline; _ = tab; _ = unicode; _ = nullChar;

            // char is numeric underneath
            int codePoint = c;        // Implicit conversion to int
            Console.WriteLine($"'A' as int: {codePoint}");  // 65

            // Char arithmetic
            char next = (char)(c + 1);  // explicit cast required for arithmetic result
            Console.WriteLine($"'A' + 1 = '{next}'");  // 'B'

            // Useful char methods
            Console.WriteLine($"char.IsLetter('A'): {char.IsLetter('A')}");
            Console.WriteLine($"char.IsDigit('5'):  {char.IsDigit('5')}");
            Console.WriteLine($"char.IsWhiteSpace(' '): {char.IsWhiteSpace(' ')}");
            Console.WriteLine($"char.ToUpper('a'): {char.ToUpper('a')}");
            Console.WriteLine($"char.ToLower('A'): {char.ToLower('A')}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // STRING
        // ═══════════════════════════════════════════════════════════════
        //
        // string — sequence of char values, reference type, IMMUTABLE
        //          Alias for System.String
        //          Default: null (not empty string)
        //          Literal: double quotes "hello"
        //
        // IMMUTABILITY — INTERVIEW CRITICAL:
        //   Every string "modification" creates a NEW string object
        //   The original string is unchanged
        //   This is why string concatenation in loops is O(n²) — avoid it
        //   Use StringBuilder for repeated concatenation
        //
        // STRING INTERNING:
        //   String literals are interned — compiler reuses same object
        //   for identical string literals. This is why "abc" == "abc" is true
        //   even for reference comparison in some cases.
        //
        // == vs .Equals() for string — INTERVIEW:
        //   string overrides == to do VALUE comparison (not reference)
        //   string == string compares content, not memory address
        //   This is different from other reference types
        //   .Equals() also does value comparison by default for string
        //   Use string.Equals(a, b, StringComparison.OrdinalIgnoreCase) for case-insensitive
        //
        // STRING vs string:
        //   Both identical — string is C# alias for System.String
        //   Convention: use string for variable declarations, String for static members
        //
        static void DemonstrateString()
        {
            Console.WriteLine("── STRING ─────────────────────────────────────────────");

            string s1 = "hello";
            string s2 = "world";
            string empty = string.Empty;
            string nullStr = null;
            _ = s1; _ = s2; _ = empty; _ = nullStr;

            // String interpolation
            string interpolated = $"Loan amount: {350_000:C}";

            // Verbatim string — no escape processing
            string path = @"C:\Users\Santosh\Documents";  // No need to escape backslashes

            // Raw string literal (C# 11+) — no escape sequences needed
            string raw = """
                This is a raw string
                with "quotes" and \backslashes\
                preserved literally
                """;

            // Immutability proof
            string original = "hello";
            string modified = original.ToUpper(); // Creates NEW string
            Console.WriteLine($"original: {original}"); // Still "hello"
            Console.WriteLine($"modified: {modified}"); // "HELLO"

            // String comparison — INTERVIEW
            string a = "hello";
            string b = "hello";
            Console.WriteLine($"\na == b: {a == b}");                    // True — value comparison
            Console.WriteLine($"a.Equals(b): {a.Equals(b)}");           // True

            // Case-insensitive comparison
            bool caseInsensitive = string.Equals("Hello", "hello",
                StringComparison.OrdinalIgnoreCase);
            Console.WriteLine($"Case-insensitive equal: {caseInsensitive}"); // True

            // Null checks — INTERVIEW
            string test = null;
            Console.WriteLine($"\nstring.IsNullOrEmpty(null): {string.IsNullOrEmpty(test)}");
            Console.WriteLine($"string.IsNullOrWhiteSpace(\"  \"): {string.IsNullOrWhiteSpace("  ")}");

            // StringBuilder for concatenation in loops
            var sb = new StringBuilder();
            for (int i = 0; i < 5; i++)
                sb.Append($"item{i} ");     // O(n) — single allocation
            string result = sb.ToString();
            Console.WriteLine($"\nStringBuilder result: {result}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // NULLABLE VALUE TYPES
        // ═══════════════════════════════════════════════════════════════
        //
        // Problem: value types cannot be null by default
        //          But databases often have nullable columns
        //          Need a way to represent "no value" for int, decimal, etc.
        //
        // Solution: Nullable<T> or shorthand T?
        //   int?     — nullable int    (Nullable<int>)
        //   decimal? — nullable decimal
        //   bool?    — nullable bool (true, false, OR null — three-state logic)
        //   DateTime? — nullable DateTime
        //
        // PROPERTIES:
        //   .HasValue — true if the nullable has a value, false if null
        //   .Value    — gets the value; throws InvalidOperationException if null
        //   .GetValueOrDefault() — returns value or default(T) if null
        //   .GetValueOrDefault(fallback) — returns value or fallback if null
        //
        // NULL COALESCING OPERATORS — INTERVIEW:
        //   ??   — returns left if not null, otherwise right
        //   ??=  — assigns right to left only if left is null
        //   ?.   — null conditional — short circuits to null if left is null
        //
        // C# 8+ NULLABLE REFERENCE TYPES:
        //   string  — non-nullable (compiler warns if null assigned)
        //   string? — explicitly nullable reference
        //   Enable with: #nullable enable
        //
        static void DemonstrateNullable()
        {
            Console.WriteLine("── NULLABLE TYPES ─────────────────────────────────────");

            int? nullableInt = null;
            decimal? nullableDecimal = 6.875m;
            bool? nullableBool = null;    // Three-state: true, false, null

            // Checking nullable
            Console.WriteLine($"nullableInt.HasValue: {nullableInt.HasValue}");    // false
            Console.WriteLine($"nullableDecimal.HasValue: {nullableDecimal.HasValue}"); // true
            Console.WriteLine($"nullableDecimal.Value: {nullableDecimal.Value}");  // 6.875

            // Safe access — never throws
            int safeValue = nullableInt.GetValueOrDefault();    // 0
            int withDefault = nullableInt.GetValueOrDefault(-1); // -1

            // Null coalescing
            int result1 = nullableInt ?? -1;      // -1 (nullableInt is null)
            Console.WriteLine($"nullableInt ?? -1: {result1}");

            // Null conditional
            string loanNumber = null;
            int? length = loanNumber?.Length;     // null — no NullReferenceException
            Console.WriteLine($"null?.Length: {length}");

            // Null coalescing assignment (C# 8+)
            string customerName = null;
            customerName ??= "Anonymous";          // Only assigns if null
            Console.WriteLine($"After ??=: {customerName}");

            // Nullable in conditional expressions
            bool? hasApproval = null;
            if (hasApproval == true) Console.WriteLine("Approved");
            else if (hasApproval == false) Console.WriteLine("Denied");
            else Console.WriteLine("Pending");  // null case
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // TYPE CONVERSION
        // ═══════════════════════════════════════════════════════════════
        //
        // IMPLICIT CONVERSION — automatic, no data loss possible
        //   byte → short → int → long → float → double
        //   int → long → decimal
        //   No cast required, compiler handles it
        //
        // EXPLICIT CONVERSION (CAST) — manual, potential data loss
        //   double → int truncates fractional part (not rounds)
        //   long → int may overflow
        //   Syntax: (targetType)value
        //
        // SAFE CONVERSION — TryParse pattern
        //   NEVER use Parse() on untrusted input — throws FormatException
        //   ALWAYS use TryParse() — returns bool, no exception
        //
        // CONVERT CLASS vs CAST:
        //   (int)3.9    → 3  (truncates)
        //   Convert.ToInt32(3.9) → 4  (rounds)
        //   Different behavior — know which you need
        //
        // AS OPERATOR:
        //   obj as string → returns null if cast fails (no exception)
        //   Only for reference types and nullable value types
        //
        // IS OPERATOR (pattern matching C# 7+):
        //   if (obj is string s) { use s }  — type check + cast in one
        //
        static void DemonstrateTypeConversion()
        {
            Console.WriteLine("── TYPE CONVERSION ────────────────────────────────────");

            // Implicit — widening, no data loss
            byte b = 100;
            short s = b;
            int i = s;
            long l = i;
            double d = l;
            _ = d; // suppress unused warning — chain demonstrates implicit widening

            // Explicit cast — narrowing, possible data loss
            double pi = 3.14159;
            int truncated = (int)pi;           // 3 — truncates, does NOT round
            Console.WriteLine($"(int)3.14159 = {truncated}");  // 3

            // Convert.ToInt32 ROUNDS, cast TRUNCATES
            int rounded = Convert.ToInt32(pi); // 3 — rounds (3.14 → 3, 3.5 → 4)
            Console.WriteLine($"Convert.ToInt32(3.14159) = {rounded}"); // 3

            double half = 3.5;
            Console.WriteLine($"(int)3.5 = {(int)half}");              // 3 — truncates
            Console.WriteLine($"Convert.ToInt32(3.5) = {Convert.ToInt32(half)}"); // 4 — rounds

            // TryParse — ALWAYS use for untrusted input
            string input = "12345";
            if (int.TryParse(input, out int parsed))
                Console.WriteLine($"\nParsed: {parsed}");
            else
                Console.WriteLine("Parse failed");

            // Dangerous — throws on bad input
            // int bad = int.Parse("not a number"); // FormatException

            // as operator — null on failure, no exception
            object obj = "hello";
            string str = obj as string;    // "hello"
            Console.WriteLine($"\nobj as string: {str}");

            // For value types use is pattern matching instead of as
            if (obj is int numVal)
                Console.WriteLine($"obj is int: {numVal}");
            else
                Console.WriteLine("obj is not int");

            // is operator with pattern matching (C# 7+)
            if (obj is string s2)
                Console.WriteLine($"Pattern match: {s2.ToUpper()}");

            // Switch expression pattern matching (C# 8+)
            string TypeDescription(object o) => o switch
            {
                int n => $"Integer: {n}",
                string s => $"String: {s}",
                null => "Null",
                _ => "Unknown"
            };
            Console.WriteLine(TypeDescription(obj));
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // BOXING AND UNBOXING
        // ═══════════════════════════════════════════════════════════════
        //
        // BOXING: converting a value type to object (reference type)
        //   - Allocates new object on the heap
        //   - Copies the value type's data into it
        //   - Returns a reference to the heap object
        //   - PERFORMANCE COST: heap allocation + GC pressure
        //
        // UNBOXING: extracting the value type back from object
        //   - Requires explicit cast
        //   - Must cast to the EXACT original type or InvalidCastException
        //   - Copies value from heap back to stack
        //
        // WHEN BOXING OCCURS — INTERVIEW:
        //   - Assigning value type to object variable
        //   - Passing value type to method expecting object
        //   - Adding value types to non-generic collections (ArrayList)
        //   - String.Format with value types (pre-interpolation)
        //
        // WHY GENERICS WERE INTRODUCED:
        //   List<int> — no boxing, stores int directly
        //   ArrayList — stores object, boxes every int → huge perf cost
        //   This is the primary motivation for generics in .NET
        //
        // HOW TO DETECT: use profiler (dotMemory) or ETW events
        //   Look for excessive Gen0 allocations in hot paths
        //
        static void DemonstrateBoxingUnboxing()
        {
            Console.WriteLine("── BOXING AND UNBOXING ────────────────────────────────");

            int value = 42;

            // BOXING — heap allocation occurs here
            object boxed = value;  // value type → reference type
            Console.WriteLine($"boxed: {boxed}");

            // UNBOXING — must cast to exact original type
            int unboxed = (int)boxed;  // explicit cast required
            Console.WriteLine($"unboxed: {unboxed}");

            // Wrong type unboxing throws InvalidCastException
            try
            {
                double wrong = (double)boxed; // int boxed as object, can't unbox to double
            }
            catch (InvalidCastException)
            {
                Console.WriteLine("InvalidCastException: must unbox to exact type");
            }

            // Boxing in non-generic collections — AVOID
            var arrayList = new System.Collections.ArrayList();
            arrayList.Add(1);   // boxes int → object (heap allocation)
            arrayList.Add(2);   // boxes int → object (heap allocation)
            int item = (int)arrayList[0]; // unboxes
            Console.WriteLine($"ArrayList item (boxed/unboxed): {item}");

            // Generic collections — NO boxing
            var list = new System.Collections.Generic.List<int>();
            list.Add(1);   // no boxing — stored as int directly
            list.Add(2);   // no boxing
            int genericItem = list[0]; // no unboxing needed
            Console.WriteLine($"Generic List item (no boxing): {genericItem}");
            Console.WriteLine($"\nGeneric List<int> — no boxing overhead");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // VAR AND DYNAMIC
        // ═══════════════════════════════════════════════════════════════
        //
        // VAR — compile-time type inference
        //   Type is determined at COMPILE TIME from the right-hand side
        //   Once inferred, the type is FIXED — cannot be reassigned to
        //   a different type
        //   No performance difference from explicit type — identical IL
        //   Requires initialization — var x; is invalid
        //
        // DYNAMIC — runtime type resolution
        //   Type checking deferred to RUNTIME
        //   Can hold any type and change type at runtime
        //   Performance overhead — DLR (Dynamic Language Runtime) involved
        //   No IntelliSense — errors only at runtime
        //   Use case: COM interop, ExpandoObject, consuming dynamic JSON,
        //             calling Python/IronPython from C#
        //
        // INTERVIEW — var vs dynamic:
        //   var     → static typing, compile-time safety, no overhead
        //   dynamic → dynamic typing, runtime errors, DLR overhead
        //   var is NOT dynamic. var is just type inference.
        //
        static void DemonstrateVarAndDynamic()
        {
            Console.WriteLine("── VAR AND DYNAMIC ────────────────────────────────────");

            // var — compile-time inference, type is fixed
            var number = 42;
            var name = "Santosh";
            var rate = 6.875m;
            _ = name; _ = rate; // suppress unused warnings

            // number = "hello"; // Compile error — var is int, not string

            Console.WriteLine($"var number type: {number.GetType().Name}");  // Int32
            Console.WriteLine($"var name type:   {name.GetType().Name}");    // String
            Console.WriteLine($"var rate type:   {rate.GetType().Name}");    // Decimal

            // dynamic — runtime type, can change
            dynamic d = 42;
            Console.WriteLine($"\ndynamic as int: {d}");
            d = "now I'm a string"; // Valid — dynamic can change type
            Console.WriteLine($"dynamic as string: {d}");
            d = 3.14;
            Console.WriteLine($"dynamic as double: {d}");

            // dynamic errors are RUNTIME not compile-time
            try
            {
                dynamic bad = "hello";
                int fail = bad + 1; // Compiles fine, fails at runtime
            }
            catch (Exception ex)
            {
                // RuntimeBinderException at runtime — caught as base Exception
                // Requires Microsoft.CSharp assembly reference for specific type
                Console.WriteLine($"\nRuntime dynamic error: {ex.GetType().Name}");
            }
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // SPECIAL TYPES
        // ═══════════════════════════════════════════════════════════════
        //
        // OBJECT — the root of all types
        //   Every type in C# inherits from object (System.Object)
        //   Methods available on ALL types: ToString(), Equals(),
        //   GetHashCode(), GetType()
        //
        // VOID — absence of return value
        //   Not a real type — cannot declare variables of type void
        //   Used only as return type for methods
        //
        // ENUM — named integer constants
        //   Underlying type is int by default
        //   Can specify: byte, short, long etc.
        //   Strongly typed — prevents raw integer misuse
        //
        // STRUCT — value type with members
        //   Copied on assignment like primitives
        //   No inheritance (can implement interfaces)
        //   Best for small immutable data < 16 bytes
        //   Examples: DateTime, TimeSpan, Point, Guid
        //
        // TUPLE — lightweight grouping without a class
        //   ValueTuple (C# 7+) — value type, named elements
        //   Tuple (older) — reference type, Item1 Item2 etc.
        //
        // RECORD (C# 9+) — reference type with value equality
        //   Immutable by default
        //   Built-in ToString, Equals, GetHashCode
        //   Non-destructive mutation with 'with'
        //   Perfect for DTOs and immutable data
        //
        static void DemonstrateSpecialTypes()
        {
            Console.WriteLine("── SPECIAL TYPES ──────────────────────────────────────");

            // object
            object anything = 42;
            Console.WriteLine($"GetType(): {anything.GetType()}");   // System.Int32
            Console.WriteLine($"ToString(): {anything.ToString()}"); // "42"

            // enum
            LoanStatus status = LoanStatus.Pending;
            Console.WriteLine($"\nEnum: {status}");                  // "Pending"
            Console.WriteLine($"Enum value: {(int)status}");        // 0
            Console.WriteLine($"Enum underlying: {status:D}");      // "0"

            // struct
            DateTime now = DateTime.UtcNow;
            Guid id = Guid.NewGuid();
            Console.WriteLine($"\nDateTime (struct): {now:yyyy-MM-dd}");
            Console.WriteLine($"Guid (struct): {id}");

            // ValueTuple
            var loan = (Amount: 350_000m, Rate: 6.875m, Term: 30);
            Console.WriteLine($"\nTuple Amount: {loan.Amount:C}");
            Console.WriteLine($"Tuple Rate:   {loan.Rate}%");

            // Deconstruction
            var (amount, rate, term) = loan;
            Console.WriteLine($"Deconstructed: {amount:C} at {rate}% for {term} years");

            // record
            var customer = new LoanCustomer("Santosh", 750);
            var updated = customer with { CreditScore = 780 }; // non-destructive mutation
            Console.WriteLine($"\nRecord: {customer}");
            Console.WriteLine($"Updated: {updated}");
            Console.WriteLine($"Equal: {customer == updated}"); // false — different score
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // SUPPORTING TYPES
    // ═══════════════════════════════════════════════════════════════════

    /// <summary>
    /// Enum — named integer constants with underlying type
    /// Default underlying type: int (can specify byte, short, long)
    /// Best practice: always specify explicit values for persistence/serialization
    /// </summary>
    public enum LoanStatus : byte  // byte underlying type — small range, save memory
    {
        Pending = 0,
        InReview = 1,
        Approved = 2,
        Denied = 3,
        Closed = 4
    }

    /// <summary>
    /// Record — C# 9+ reference type with value-based equality
    /// Positional record: compiler generates constructor, properties,
    /// Equals, GetHashCode, ToString, Deconstruct automatically
    ///
    /// INTERVIEW: record vs class vs struct
    ///   record  → reference type, value equality, immutable by default, DTOs
    ///   class   → reference type, reference equality, mutable, complex objects
    ///   struct  → value type, value equality, copied, small immutable data
    /// </summary>
    public record LoanCustomer(string Name, int CreditScore);

    /// <summary>
    /// Struct — value type with members
    /// Use when: small (&lt;16 bytes), immutable, frequently allocated,
    ///           performance-critical inner loops
    /// Avoid when: large, mutable, needs inheritance
    /// </summary>
    public struct LoanAmount
    {
        public decimal Principal { get; init; }
        public string Currency { get; init; }

        public LoanAmount(decimal principal, string currency = "USD")
        {
            Principal = principal;
            Currency = currency;
        }

        public override string ToString() => $"{Principal:C} {Currency}";
    }
}
