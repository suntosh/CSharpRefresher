/// <summary>
/// Advanced C# Syntax — Interview Reference
/// ═══════════════════════════════════════════════════════════════════
///
/// Covers the patterns you see in modern C# codebases that look
/// unfamiliar if you've been heads-down in CUDA and systems code:
///
///   - Records and positional parameters
///   - Implicit and explicit operator overloading
///   - Expression-bodied members =>
///   - Primary constructors
///   - Init-only properties
///   - Pattern matching advanced forms
///   - Deconstruction
///   - Local functions
///   - Target-typed new()
///   - Caller information attributes
///   - Covariant return types
///   - Default interface methods
///
/// ═══════════════════════════════════════════════════════════════════
/// </summary>

using System;
using System.Runtime.CompilerServices;

namespace Refresher.AdvancedSyntax
{
    public class Sugar
    {
        public static void Exec()
        {
            Console.WriteLine("Advanced C# Syntax Reference\n");

            DemonstrateRecords();
            DemonstrateOperatorOverloading();
            DemonstrateExpressionBodied();
            DemonstratePrimaryConstructors();
            DemonstrateInitOnly();
            DemonstratePatternMatching();
            DemonstrateDeconstruction();
            DemonstrateLocalFunctions();
            DemonstrateTargetTypedNew();
            DemonstrateCallerInfo();
            DemonstrateDefaultInterfaceMethods();
        }

        // ═══════════════════════════════════════════════════════════════
        // RECORDS AND POSITIONAL PARAMETERS — THE CODE YOU SAW
        // ═══════════════════════════════════════════════════════════════
        //
        // The exact pattern from the snippet:
        //
        //   public record DieType(int Sides, int Value = 0)
        //
        // Breaking it down:
        //
        // 1. RECORD — reference type with VALUE-BASED equality
        //    Compiler generates: constructor, properties, Equals,
        //    GetHashCode, ToString, Deconstruct — all automatically
        //
        // 2. POSITIONAL PARAMETERS — (int Sides, int Value = 0)
        //    These become:
        //      - Constructor parameters: new DieType(6)
        //      - Init-only properties:   d.Sides, d.Value
        //      - Deconstruct method:     var (sides, val) = d
        //
        // 3. DEFAULT PARAMETER VALUE — int Value = 0
        //    Optional parameter — DieType(6) sets Value to 0 automatically
        //
        // 4. IMPLICIT OPERATOR
        //    public static implicit operator int(DieType d) => d.Sides;
        //    Allows: int x = someDieType; (no cast needed)
        //
        // 5. EXPLICIT OPERATOR
        //    public static explicit operator DieType(int i) => new(i);
        //    Allows: DieType d = (DieType)6; (cast required)
        //    new(i) is TARGET-TYPED NEW — compiler infers DieType from context
        //
        // RECORD vs CLASS vs STRUCT:
        //   record  — reference type, value equality, immutable by default
        //   class   — reference type, reference equality, mutable
        //   struct  — value type, value equality, copied on assignment
        //
        // record struct (C# 10+):
        //   Value type + value equality + record features
        //
        static void DemonstrateRecords()
        {
            Console.WriteLine("── RECORDS ────────────────────────────────────────────");

            // Positional record — compiler generates everything
            var d6 = new DieType(6);         // Value defaults to 0
            var d20 = new DieType(20, 5);     // Both params

            Console.WriteLine($"d6:  Sides={d6.Sides},  Value={d6.Value}");
            Console.WriteLine($"d20: Sides={d20.Sides}, Value={d20.Value}");

            // VALUE-BASED EQUALITY — records compare by content not reference
            var a = new DieType(6);
            var b = new DieType(6);
            Console.WriteLine($"\na == b:              {a == b}");            // True
            Console.WriteLine($"ReferenceEqual(a,b): {ReferenceEquals(a, b)}"); // False — different objects

            // WITH — non-destructive mutation, returns new record
            var d6upgraded = d6 with { Value = 3 };
            Console.WriteLine($"\nd6 unchanged:   {d6}");          // DieType { Sides = 6, Value = 0 }
            Console.WriteLine($"d6upgraded:     {d6upgraded}");   // DieType { Sides = 6, Value = 3 }

            // DECONSTRUCT — generated automatically for positional records
            var (sides, value) = d20;
            Console.WriteLine($"\nDeconstructed: sides={sides}, value={value}");

            // ToString — generated automatically
            Console.WriteLine($"ToString: {d6}"); // DieType { Sides = 6, Value = 0 }

            // IMPLICIT OPERATOR — DieType → int (no cast needed)
            int sideCount = d6;               // Implicit — calls operator int(DieType d)
            Console.WriteLine($"\nImplicit int: {sideCount}");    // 6

            // EXPLICIT OPERATOR — int → DieType (cast required)
            DieType fromInt = (DieType)12;    // Explicit — calls operator DieType(int i)
            Console.WriteLine($"Explicit DieType: {fromInt}");   // DieType { Sides = 12, Value = 0 }

            // Inheritance — records support it
            var special = new SpecialDie(6, "Fire");
            Console.WriteLine($"\nDerived record: {special}");

            // record struct (C# 10+) — value type record
            var point = new Point3D(1.0f, 2.0f, 3.0f);
            var point2 = point with { Z = 99.0f };
            Console.WriteLine($"\nrecord struct: {point}");
            Console.WriteLine($"with mutation: {point2}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // OPERATOR OVERLOADING — IMPLICIT AND EXPLICIT
        // ═══════════════════════════════════════════════════════════════
        //
        // IMPLICIT OPERATOR:
        //   No cast required — compiler inserts conversion automatically
        //   Should NEVER throw, NEVER lose information
        //   Use when conversion is always safe and obvious
        //   Example: int → long (always safe), DieType → int (returns Sides)
        //
        // EXPLICIT OPERATOR:
        //   Cast required: (TargetType)value
        //   MAY throw, MAY lose information
        //   Use when conversion requires conscious intent from caller
        //   Example: double → int (truncates), int → DieType (could be invalid)
        //
        // ARITHMETIC OPERATORS:
        //   +, -, *, /, % — binary operators
        //   ++, -- — unary operators
        //   ==, != — must be defined in pairs
        //   <, > and <=, >= — must be defined in pairs
        //
        // INTERVIEW: Why have both implicit and explicit?
        //   Implicit = safe, automatic, no information loss
        //   Explicit = risky, intentional, caller acknowledges the conversion
        //   Using implicit for lossy conversions is an antipattern
        //
        static void DemonstrateOperatorOverloading()
        {
            Console.WriteLine("── OPERATOR OVERLOADING ───────────────────────────────");

            var usd100 = new Money(100m, "USD");
            var usd50 = new Money(50m, "USD");

            // Arithmetic operators
            Money sum = usd100 + usd50;
            Money diff = usd100 - usd50;
            Console.WriteLine($"100 + 50 = {sum}");   // $150.00 USD
            Console.WriteLine($"100 - 50 = {diff}");  // $50.00 USD

            // Comparison operators
            Console.WriteLine($"100 > 50:  {usd100 > usd50}");   // True
            Console.WriteLine($"100 == 50: {usd100 == usd50}");  // False
            Console.WriteLine($"100 == 100: {usd100 == new Money(100m, "USD")}"); // True

            // Implicit conversion Money → decimal
            decimal amount = usd100;       // Implicit — no cast
            Console.WriteLine($"\nImplicit →decimal: {amount}"); // 100

            // Explicit conversion decimal → Money (assumes USD)
            Money fromDecimal = (Money)250m; // Explicit — cast required
            Console.WriteLine($"Explicit →Money: {fromDecimal}");

            // Unary operators
            Money negated = -usd50;
            Console.WriteLine($"Negated: {negated}"); // -$50.00 USD

            // Increment/decrement
            var m = new Money(10m, "USD");
            m++;
            Console.WriteLine($"After ++: {m}"); // $11.00 USD
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // EXPRESSION-BODIED MEMBERS — THE => SYNTAX
        // ═══════════════════════════════════════════════════════════════
        //
        // Expression body replaces { return expr; } with => expr
        // Works on: methods, properties, constructors, destructors,
        //           indexers, operators, local functions
        //
        // WHEN TO USE:
        //   Single expression that returns a value — always use =>
        //   Multiple statements — use block body { }
        //   Side effects only (void) — can still use =>, just no return
        //
        // INTERVIEW: What is the difference between => in lambda and => in member?
        //   Lambda:        x => x * 2         (creates a delegate/Func)
        //   Member body:   public int X => 5; (defines the implementation)
        //   Same token, different context — compiler knows from location
        //
        static void DemonstrateExpressionBodied()
        {
            Console.WriteLine("── EXPRESSION-BODIED MEMBERS ──────────────────────────");

            var c = new Circle(5.0);
            Console.WriteLine($"Area:        {c.Area:F2}");        // 78.54
            Console.WriteLine($"Circumference: {c.Circumference:F2}"); // 31.42
            Console.WriteLine($"ToString:    {c}");               // Circle(r=5.00)
            Console.WriteLine($"IsLarge:     {c.IsLarge}");        // False

            // Expression-bodied indexer
            var matrix = new Matrix(3);
            matrix[0, 0] = 1.0;
            matrix[1, 1] = 2.0;
            Console.WriteLine($"\nMatrix[0,0]: {matrix[0, 0]}");
            Console.WriteLine($"Matrix[1,1]: {matrix[1, 1]}");

            // Expression-bodied local function
            static double Square(double x) => x * x;
            Console.WriteLine($"\nSquare(5): {Square(5)}"); // 25
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // PRIMARY CONSTRUCTORS (C# 12)
        // ═══════════════════════════════════════════════════════════════
        //
        // Parameters declared directly on the class/struct declaration
        // Parameters are in scope for the entire class body
        // Eliminates boilerplate of storing constructor params in fields
        //
        // NOTE: Different from record positional parameters
        //   Records:              auto-generate properties from params
        //   Primary constructors: params available but no auto-properties
        //                         you must explicitly use them
        //
        // INTERVIEW: Primary constructor vs record?
        //   Record: params → auto init-only properties + value equality + with
        //   Primary constructor on class: params available in body, no auto-props
        //
        static void DemonstratePrimaryConstructors()
        {
            Console.WriteLine("── PRIMARY CONSTRUCTORS (C# 12) ───────────────────────");

            var service = new LoanService("https://api.loans.com", 30);
            Console.WriteLine($"BaseUrl: {service.BaseUrl}");
            Console.WriteLine($"Timeout: {service.TimeoutSeconds}s");
            service.Process("LOAN-001");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // INIT-ONLY PROPERTIES — init accessor
        // ═══════════════════════════════════════════════════════════════
        //
        // init — like set but ONLY settable during object initialization
        // After construction the property is read-only
        // Enables immutable types that still work with object initializers
        //
        // USE WITH:
        //   Object initializer syntax: new Thing { Prop = val }
        //   record with expressions
        //   Deserialization (System.Text.Json supports init setters)
        //
        // INTERVIEW: init vs set vs readonly field?
        //   readonly field: set in constructor only, never via property syntax
        //   set:            settable anytime after construction
        //   init:           settable during construction/object-init only
        //
        static void DemonstrateInitOnly()
        {
            Console.WriteLine("── INIT-ONLY PROPERTIES ───────────────────────────────");

            // Object initializer with init properties
            var app = new LoanApplication
            {
                ApplicationId = "APP-2024-001",
                BorrowerName = "Alice Smith",
                Amount = 350_000m,
                SubmittedAt = DateTime.UtcNow
            };

            Console.WriteLine($"Id:        {app.ApplicationId}");
            Console.WriteLine($"Borrower:  {app.BorrowerName}");
            Console.WriteLine($"Amount:    {app.Amount:C}");

            // After construction — cannot set init properties
            // app.ApplicationId = "different"; // CS8852 — init-only
            Console.WriteLine("Init-only: cannot reassign after construction");

            // Required init (C# 11) — must be set during initialization
            var config = new ServiceConfig
            {
                ApiKey = "key-12345",  // required — compiler error if omitted
                Region = "us-east-1"   // required
            };
            Console.WriteLine($"\nRequired init: {config.ApiKey}, {config.Region}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // PATTERN MATCHING — ADVANCED FORMS
        // ═══════════════════════════════════════════════════════════════
        //
        // C# 7-11 added progressively richer pattern matching
        //
        // PATTERN TYPES:
        //   Type pattern:       obj is string s
        //   Declaration:        obj is int i
        //   Constant:           x is 42
        //   Relational:         x is > 0 and < 100
        //   Logical:            x is > 0 and not null
        //   Property:           obj is { Name: "Alice", Age: > 18 }
        //   Positional:         point is (0, 0)
        //   List (C# 11):       arr is [1, 2, ..]
        //   var:                obj is var x (always matches)
        //   Discard:            obj is _
        //
        static void DemonstratePatternMatching()
        {
            Console.WriteLine("── PATTERN MATCHING ───────────────────────────────────");

            // Relational patterns
            static string ClassifyRate(double rate) => rate switch
            {
                < 0 => "Invalid",
                0 => "Zero rate",
                > 0 and < 3 => "Low",
                >= 3 and < 7 => "Moderate",
                >= 7 and < 10 => "High",
                _ => "Very high"
            };

            Console.WriteLine($"2.5% → {ClassifyRate(2.5)}");
            Console.WriteLine($"6.9% → {ClassifyRate(6.9)}");
            Console.WriteLine($"8.5% → {ClassifyRate(8.5)}");

            // Property patterns
            var loan = new { Status = "APPROVED", Amount = 350_000m, IsJumbo = true };
            string message = loan switch
            {
                { Status: "APPROVED", IsJumbo: true } => "Approved jumbo — escalate",
                { Status: "APPROVED", IsJumbo: false } => "Standard approval",
                { Status: "DENIED" } => "Application denied",
                { Status: "PENDING" } => "Under review",
                _ => "Unknown status"
            };
            Console.WriteLine($"\nProperty pattern: {message}");

            // Logical patterns — and, or, not
            static bool IsValidAge(int age) => age is >= 18 and <= 120;
            static bool IsExtremeRate(double r) => r is < 1.0 or > 15.0;
            static bool IsNotNull(object? o) => o is not null;

            Console.WriteLine($"\nIsValidAge(25):     {IsValidAge(25)}");     // True
            Console.WriteLine($"IsValidAge(200):    {IsValidAge(200)}");    // False
            Console.WriteLine($"IsExtremeRate(0.5): {IsExtremeRate(0.5)}"); // True
            Console.WriteLine($"IsNotNull(null):    {IsNotNull(null)}");    // False

            // Positional pattern — uses Deconstruct
            var point = new Point(3, 4);
            string quadrant = point switch
            {
                (0, 0) => "Origin",
                ( > 0, > 0) => "Quadrant I",
                ( < 0, > 0) => "Quadrant II",
                ( < 0, < 0) => "Quadrant III",
                ( > 0, < 0) => "Quadrant IV",
                _ => "On axis"
            };
            Console.WriteLine($"\nPositional pattern: ({point.X},{point.Y}) → {quadrant}");

            // List patterns (C# 11)
            int[] arr1 = { 1, 2, 3 };
            int[] arr2 = { 1 };
            int[] arr3 = { };

            static string DescribeArray(int[] a) => a switch
            {
                [] => "Empty",
                [var x] => $"Single: {x}",
                [var x, var y] => $"Two: {x},{y}",
                [1, 2, ..] => "Starts with 1,2",
                [.., 99] => "Ends with 99",
                _ => $"Many ({a.Length})"
            };

            Console.WriteLine($"\nList pattern [1,2,3]: {DescribeArray(arr1)}");
            Console.WriteLine($"List pattern [1]:     {DescribeArray(arr2)}");
            Console.WriteLine($"List pattern []:      {DescribeArray(arr3)}");
            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // DECONSTRUCTION
        // ═══════════════════════════════════════════════════════════════
        //
        // Any type can support deconstruction by implementing Deconstruct()
        // Records with positional params get Deconstruct() automatically
        // Tuples deconstruct automatically
        // Custom classes need explicit Deconstruct method
        //
        static void DemonstrateDeconstruction()
        {
            Console.WriteLine("── DECONSTRUCTION ─────────────────────────────────────");

            // Tuple deconstruction
            var (name, age) = GetPerson();
            Console.WriteLine($"Tuple: {name}, {age}");

            // Ignore parts with discard _
            var (firstName, _) = GetPerson();
            Console.WriteLine($"Discard: {firstName}");

            // Custom class with Deconstruct
            var loan = new LoanSummary("LOAN-001", 350_000m, 6.875m);
            var (id, amount, rate) = loan;
            Console.WriteLine($"Custom: {id}, {amount:C}, {rate}%");

            // Nested deconstruction
            var ((nested_name, nested_age), score) = (GetPerson(), 95);
            Console.WriteLine($"Nested: {nested_name}, {nested_age}, score={score}");

            // Deconstruction in foreach
            var loans = new[]
            {
                new LoanSummary("L1", 100_000m, 5.5m),
                new LoanSummary("L2", 200_000m, 6.0m),
            };

            foreach (var (loanId, loanAmount, loanRate) in loans)
                Console.WriteLine($"  {loanId}: {loanAmount:C} at {loanRate}%");

            Console.WriteLine();
        }

        static (string Name, int Age) GetPerson() => ("Alice", 30);

        // ═══════════════════════════════════════════════════════════════
        // LOCAL FUNCTIONS
        // ═══════════════════════════════════════════════════════════════
        //
        // Functions defined inside another method
        // Can access variables from enclosing scope (closure)
        // Can be static (no closure) — preferred for performance
        // Can be async, generic, recursive
        //
        // vs LAMBDA:
        //   Local function: named, can be recursive, no allocation overhead
        //   Lambda:         anonymous, allocated as delegate object on heap
        //   For recursive helpers: must use local function (lambda can't self-reference)
        //   For passing as callback: use lambda (it's a delegate)
        //
        static void DemonstrateLocalFunctions()
        {
            Console.WriteLine("── LOCAL FUNCTIONS ────────────────────────────────────");

            // Basic local function
            double result = Calculate(10, 20);
            Console.WriteLine($"Calculate: {result}");

            // Recursive local function
            int fib = Fibonacci(10);
            Console.WriteLine($"Fibonacci(10): {fib}");

            // Static local function — no closure, no allocation
            static double Hypotenuse(double a, double b) =>
                Math.Sqrt(a * a + b * b);
            Console.WriteLine($"Hypotenuse(3,4): {Hypotenuse(3, 4)}");

            // Local function accessing enclosing scope
            double tax = 0.15;
            decimal ApplyTax(decimal amount)
            {
                return amount * (1 + (decimal)tax); // captures tax from enclosing scope
            }
            Console.WriteLine($"ApplyTax(100): {ApplyTax(100m):C}");

            // Iterator local function
            foreach (var n in Range(1, 5))
                Console.Write($"{n} ");
            Console.WriteLine();

            static IEnumerable<int> Range(int start, int count)
            {
                for (int i = 0; i < count; i++)
                    yield return start + i;
            }

            double Calculate(double a, double b)
            {
                double squared = Square(a) + Square(b); // local calls local
                return Math.Sqrt(squared);
                static double Square(double x) => x * x;
            }

            int Fibonacci(int n)
            {
                if (n <= 1) return n;
                return Fibonacci(n - 1) + Fibonacci(n - 2); // recursive local
            }

            Console.WriteLine();
        }

        // ═══════════════════════════════════════════════════════════════
        // TARGET-TYPED NEW — new()
        // ═══════════════════════════════════════════════════════════════
        //
        // C# 9+ — compiler infers the type from context
        // Use when the type is already obvious from the left side
        //
        // RULES:
        //   Left side must be a concrete type (not var, not interface)
        //   Works in: field initializers, method params, return statements
        //
        static void DemonstrateTargetTypedNew()
        {
            Console.WriteLine("── TARGET-TYPED NEW ───────────────────────────────────");

            // Traditional
            System.Collections.Generic.List<string> list1 =
                new System.Collections.Generic.List<string>();

            // Target-typed new — type inferred from left side
            System.Collections.Generic.List<string> list2 = new();
            System.Collections.Generic.Dictionary<string, int> dict = new();

            list2.Add("item");
            dict["key"] = 1;

            // In method parameters
            ProcessList(new() { "a", "b", "c" });

            // In record/object initializers
            var app = new LoanApplication
            {
                ApplicationId = "APP-001",
                BorrowerName = "Bob",
                Amount = 100_000m,
                SubmittedAt = new()  // DateTime — target-typed
            };

            // As return type (when return type is declared)
            static System.Collections.Generic.List<int> MakeList() => new() { 1, 2, 3 };
            var nums = MakeList();
            Console.WriteLine($"Target-typed list: [{string.Join(",", nums)}]");
            Console.WriteLine();
        }

        static void ProcessList(System.Collections.Generic.List<string> items)
            => Console.WriteLine($"Processing {items.Count} items");

        // ═══════════════════════════════════════════════════════════════
        // CALLER INFORMATION ATTRIBUTES
        // ═══════════════════════════════════════════════════════════════
        //
        // Automatically inject caller context at compile time
        // Zero runtime cost — values baked in at compile time
        //
        // [CallerMemberName]  — name of calling method/property
        // [CallerFilePath]    — source file path
        // [CallerLineNumber]  — line number of the call
        // [CallerArgumentExpression] (C# 10) — expression passed as argument
        //
        // USE CASES:
        //   Logging — auto-capture method name without reflection
        //   INotifyPropertyChanged — property name without magic strings
        //   Debug assertions — show what expression was evaluated
        //
        static void DemonstrateCallerInfo()
        {
            Console.WriteLine("── CALLER INFORMATION ATTRIBUTES ──────────────────────");

            Log("Application started");
            Log("Processing loan");

            // INotifyPropertyChanged pattern
            var vm = new LoanViewModel();
            vm.Amount = 350_000m;   // Internally uses CallerMemberName

            // CallerArgumentExpression (C# 10)
            int value = -5;
            Assert(value > 0, value); // Shows "value > 0" failed for value=-5
            Console.WriteLine();
        }

        static void Log(string message,
            [CallerMemberName] string caller = "",
            [CallerLineNumber] int line = 0)
        {
            Console.WriteLine($"  [{caller}:{line}] {message}");
        }

        static void Assert(bool condition, int value,
            [CallerArgumentExpression("condition")] string expr = "")
        {
            if (!condition)
                Console.WriteLine($"  Assert failed: '{expr}' — value was {value}");
        }

        // ═══════════════════════════════════════════════════════════════
        // DEFAULT INTERFACE METHODS (C# 8+)
        // ═══════════════════════════════════════════════════════════════
        //
        // Interfaces can now have method implementations
        // Allows adding new methods to interfaces without breaking implementors
        // Implementing class can override, or inherit the default
        //
        // USE CASES:
        //   API evolution — add methods to published interfaces
        //   Mixin-style behavior
        //   Reduce boilerplate in implementors
        //
        // INTERVIEW: Why not just use abstract class?
        //   A class can implement multiple interfaces — can't inherit multiple classes
        //   Default interface methods enable multiple inheritance of behavior
        //   Abstract class adds state — interfaces remain stateless
        //
        static void DemonstrateDefaultInterfaceMethods()
        {
            Console.WriteLine("── DEFAULT INTERFACE METHODS ──────────────────────────");

            ILoanProcessor processor1 = new StandardProcessor();
            ILoanProcessor processor2 = new ExpressProcessor();

            // Both use the default Validate — not overridden
            Console.WriteLine($"Standard valid: {processor1.Validate("LOAN-001")}");
            Console.WriteLine($"Express valid:  {processor2.Validate("LOAN-001")}");

            // Process — ExpressProcessor overrides it
            processor1.Process("LOAN-001");
            processor2.Process("LOAN-001");

            // Default method with logic
            Console.WriteLine($"Standard fee: {processor1.CalculateFee(350_000m):C}");
            Console.WriteLine($"Express fee:  {processor2.CalculateFee(350_000m):C}");
            Console.WriteLine();
        }
    }

    // ═══════════════════════════════════════════════════════════════════
    // SUPPORTING TYPES
    // ═══════════════════════════════════════════════════════════════════

    // THE EXACT PATTERN FROM YOUR SNIPPET
    public record DieType(int Sides, int Value = 0)
    {
        // IMPLICIT OPERATOR — DieType → int, no cast needed
        // Safe: always returns a valid int (Sides)
        public static implicit operator int(DieType d) => d.Sides;

        // EXPLICIT OPERATOR — int → DieType, cast required
        // Explicit because caller must decide: what does an int mean as a DieType?
        // new(i) = target-typed new — compiler infers DieType from context
        public static explicit operator DieType(int i) => new(i);
    }

    // Record inheritance
    public record SpecialDie(int Sides, string Element) : DieType(Sides);

    // record struct (C# 10) — value type + record features
    public record struct Point3D(float X, float Y, float Z);

    // MONEY — demonstrates operator overloading fully
    public readonly struct Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }

        // Arithmetic
        public static Money operator +(Money a, Money b)
        {
            if (a.Currency != b.Currency)
                throw new InvalidOperationException("Currency mismatch");
            return new(a.Amount + b.Amount, a.Currency);
        }

        public static Money operator -(Money a, Money b)
            => new(a.Amount - b.Amount, a.Currency);

        public static Money operator -(Money m)
            => new(-m.Amount, m.Currency);

        public static Money operator ++(Money m)
            => new(m.Amount + 1, m.Currency);

        // Comparison — must define == and != as a pair
        public static bool operator ==(Money a, Money b)
            => a.Amount == b.Amount && a.Currency == b.Currency;

        public static bool operator !=(Money a, Money b) => !(a == b);

        public static bool operator >(Money a, Money b)
            => a.Amount > b.Amount;

        public static bool operator <(Money a, Money b)
            => a.Amount < b.Amount;

        public static bool operator >=(Money a, Money b)
            => a.Amount >= b.Amount;

        public static bool operator <=(Money a, Money b)
            => a.Amount <= b.Amount;

        // IMPLICIT — Money → decimal (safe, no info loss)
        public static implicit operator decimal(Money m) => m.Amount;

        // EXPLICIT — decimal → Money (need to know currency, assumes USD)
        public static explicit operator Money(decimal d) => new(d, "USD");

        public override string ToString() => $"{Amount:C} {Currency}";
        public override bool Equals(object? obj) => obj is Money m && this == m;
        public override int GetHashCode() => HashCode.Combine(Amount, Currency);
    }

    // CIRCLE — expression-bodied members
    public class Circle
    {
        private readonly double _radius;

        public Circle(double radius) => _radius = radius; // expression-bodied constructor

        // Expression-bodied properties
        public double Area => Math.PI * _radius * _radius;
        public double Circumference => 2 * Math.PI * _radius;
        public bool IsLarge => _radius > 10;

        // Expression-bodied method
        public double ScaledArea(double factor) => Area * factor;

        // Expression-bodied override
        public override string ToString() => $"Circle(r={_radius:F2})";
    }

    // MATRIX — expression-bodied indexer
    public class Matrix
    {
        private readonly double[,] _data;
        public Matrix(int size) => _data = new double[size, size];

        // Expression-bodied indexer
        public double this[int r, int c]
        {
            get => _data[r, c];
            set => _data[r, c] = value;
        }
    }

    // PRIMARY CONSTRUCTOR (C# 12)
    public class LoanService(string baseUrl, int timeoutSeconds)
    {
        // Parameters baseUrl and timeoutSeconds are in scope throughout
        public string BaseUrl => baseUrl;
        public int TimeoutSeconds => timeoutSeconds;

        public void Process(string loanId) =>
            Console.WriteLine($"  Processing {loanId} via {baseUrl} (timeout={timeoutSeconds}s)");
    }

    // INIT-ONLY PROPERTIES
    public class LoanApplication
    {
        public string ApplicationId { get; init; } = "";  // init-only
        public string BorrowerName { get; init; } = "";
        public decimal Amount { get; init; }
        public DateTime SubmittedAt { get; init; }
    }

    // REQUIRED INIT (C# 11)
    public class ServiceConfig
    {
        public required string ApiKey { get; init; }  // must be set at init
        public required string Region { get; init; }  // must be set at init
        public int TimeoutMs { get; init; } = 5000; // optional
    }

    // POINT for positional pattern matching
    public class Point
    {
        public int X { get; }
        public int Y { get; }
        public Point(int x, int y) { X = x; Y = y; }
        public void Deconstruct(out int x, out int y) { x = X; y = Y; }
    }

    // LOAN SUMMARY — custom deconstruction
    public class LoanSummary
    {
        public string Id { get; }
        public decimal Amount { get; }
        public decimal Rate { get; }

        public LoanSummary(string id, decimal amount, decimal rate)
        {
            Id = id; Amount = amount; Rate = rate;
        }

        // Deconstruct — enables var (id, amount, rate) = loan
        public void Deconstruct(out string id, out decimal amount, out decimal rate)
        {
            id = Id; amount = Amount; rate = Rate;
        }
    }

    // CALLER INFO — INotifyPropertyChanged pattern
    public class LoanViewModel
    {
        private decimal _amount;

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(); // CallerMemberName captures "Amount" automatically
            }
        }

        private void OnPropertyChanged([CallerMemberName] string property = "")
            => Console.WriteLine($"  Property changed: {property}");
    }

    // DEFAULT INTERFACE METHODS
    public interface ILoanProcessor
    {
        void Process(string loanId);

        // Default implementation — implementors inherit this unless they override
        bool Validate(string loanId) => !string.IsNullOrEmpty(loanId);

        // Default fee calculation — can be overridden per processor
        decimal CalculateFee(decimal amount) => amount * 0.001m; // 0.1% default
    }

    public class StandardProcessor : ILoanProcessor
    {
        public void Process(string loanId) =>
            Console.WriteLine($"  Standard processing: {loanId}");
        // Validate and CalculateFee inherited from interface default
    }

    public class ExpressProcessor : ILoanProcessor
    {
        public void Process(string loanId) =>
            Console.WriteLine($"  EXPRESS processing: {loanId} (priority queue)");

        // Override the default fee — express costs more
        public decimal CalculateFee(decimal amount) => amount * 0.003m; // 0.3%
        // Validate still uses interface default
    }
}