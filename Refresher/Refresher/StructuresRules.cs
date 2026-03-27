

namespace Refresher
{
    /*
    ========================================================================
    RULE TABLE

    | Type / Modifier      | Declarable | Instantiable | Inheritable | Notes |
    |----------------------|------------|--------------|-------------|-------|
    | class                | Yes        | Yes          | Yes         | Default reference type |
    | abstract class       | Yes        | No           | Yes         | Must be derived to instantiate |
    | sealed class         | Yes        | Yes          | No          | Final class |
    | static class         | Yes        | No           | No          | Only static members |
    | internal class       | Yes        | Yes          | Yes         | Visible only inside same assembly |
    | partial class        | Yes        | Yes          | Yes         | Split across declarations/files |
    | interface            | Yes        | No           | N/A*        | Implemented by classes/structs; interfaces can inherit interfaces |
    | struct               | Yes        | Yes          | No          | Value type; can implement interfaces |
    | enum                 | Yes        | No**         | No          | Named constants |
    | record class         | Yes        | Yes          | Yes         | Reference type with value equality |
    | record struct        | Yes        | Yes          | No          | Value type with record syntax |
    | delegate             | Yes        | Yes***       | No****      | Method signature type |

    * "Inheritable" for interface means interface-to-interface inheritance, not class inheritance.
    ** Enum values are used directly; not instantiated like normal classes with new in practice.
    *** A delegate instance is created by assigning a method/lambda.
    **** Not used as a normal inheritance target.

    ========================================================================
    COMMON LEGAL / ILLEGAL COMBINATIONS

    Valid:
    - public static class
    - internal static class
    - public abstract class
    - internal abstract class
    - public partial class
    - public abstract partial class
    - public sealed class
    - internal sealed class
    - public partial struct
    - internal enum
    - public record class
    - public record struct

    Invalid:
    - public abstract sealed class    // invalid for normal class design
    - static abstract class           // invalid
    - private top-level class         // invalid
    - protected top-level class       // invalid
    - class C : A, B                  // invalid: multiple class inheritance not allowed
    - class X : SomeStruct            // invalid: cannot inherit from struct
    - class Y : SomeStaticClass       // invalid: cannot inherit from static class
    - class Z : SomeSealedClass       // invalid: cannot inherit from sealed class

    ========================================================================
    QUICK MENTAL MODEL

    - abstract = must inherit to use
    - sealed   = cannot inherit from this
    - static   = no instances, no inheritance, only static members
    - partial  = same type split into multiple declarations
    - internal = visible only inside same assembly
    - interface = contract, not concrete implementation
    ========================================================================
    */

    // =========================================================
    // 1. INTERFACE
    // Declarable: yes
    // Instantiable: no
    // Inheritable: interface-to-interface only
    // Implementable: yes, by classes and structs
    // =========================================================
    public interface ILogger
    {
        void Log(string message);
    }

    public interface IAdvancedLogger : ILogger
    {
        void LogError(string message);
    }

    // =========================================================
    // 2. ABSTRACT CLASS
    // Declarable: yes
    // Instantiable: no
    // Inheritable: yes
    // =========================================================
    public abstract class Animal
    {
        public abstract void Speak();

        public virtual void Sleep()
        {
            Console.WriteLine("Sleeping...");
        }
    }

    // =========================================================
    // 3. NORMAL PUBLIC CLASS
    // Declarable: yes
    // Instantiable: yes
    // Inheritable: yes, unless sealed
    // =========================================================
    public class Dog : Animal, ILogger
    {
        public override void Speak()
        {
            Console.WriteLine("Woof");
        }

        public void Log(string message)
        {
            Console.WriteLine($"Dog log: {message}");
        }
    }

    // =========================================================
    // 4. SEALED CLASS
    // Declarable: yes
    // Instantiable: yes
    // Inheritable: no
    // =========================================================
    public sealed class FinalUtility
    {
        public void Run()
        {
            Console.WriteLine("FinalUtility.Run");
        }
    }

    // INVALID:
    // public class ChildOfFinalUtility : FinalUtility { }
    // Reason: sealed classes cannot be inherited

    // =========================================================
    // 5. STATIC CLASS
    // Declarable: yes
    // Instantiable: no
    // Inheritable: no
    // Must contain only static members
    // =========================================================
    public static class MathHelper
    {
        public static int Add(int a, int b) => a + b;
    }

    // INVALID:
    // public class MyMathHelper : MathHelper { }
    // Reason: static classes cannot be inherited

    // INVALID:
    // var x = new MathHelper();
    // Reason: static classes cannot be instantiated

    // =========================================================
    // 6. INTERNAL CLASS
    // Declarable: yes
    // Instantiable: yes
    // Inheritable: yes, subject to accessibility
    // Visibility: same assembly only
    // =========================================================
    internal class InternalWorker
    {
        public void Work()
        {
            Console.WriteLine("Internal worker");
        }
    }

    // =========================================================
    // 7. INTERNAL STATIC CLASS
    // Declarable: yes
    // Instantiable: no
    // Inheritable: no
    // Visibility: same assembly only
    // =========================================================
    internal static class InternalTools
    {
        public static void Ping()
        {
            Console.WriteLine("Ping");
        }
    }

    // =========================================================
    // 8. PARTIAL CLASS
    // Declarable: yes
    // Instantiable: yes
    // Inheritable: yes, unless sealed/static
    // Can be split across multiple declarations/files
    // =========================================================
    public partial class Customer
    {
        public string Name { get; set; } = "";
    }

    public partial class Customer
    {
        public void Print()
        {
            Console.WriteLine($"Customer: {Name}");
        }
    }

    // =========================================================
    // 9. ABSTRACT + PARTIAL CLASS
    // Declarable: yes
    // Instantiable: no
    // Inheritable: yes
    // =========================================================
    public abstract partial class Device
    {
        public abstract void Start();
    }

    public abstract partial class Device
    {
        public void Reset()
        {
            Console.WriteLine("Device reset");
        }
    }

    public class Router : Device
    {
        public override void Start()
        {
            Console.WriteLine("Router started");
        }
    }

    // =========================================================
    // 10. STRUCT
    // Declarable: yes
    // Instantiable: yes
    // Inheritable as base type: no
    // Can implement interfaces: yes
    // Can be partial: yes
    // =========================================================
    public struct Point : ILogger
    {
        public int X;
        public int Y;

        public void Log(string message)
        {
            Console.WriteLine($"Point log: {message}");
        }
    }

    // INVALID:
    // public class MyPoint : Point { }
    // Reason: cannot inherit from a struct

    // =========================================================
    // 11. ENUM
    // Declarable: yes
    // Instantiable: not like a normal class
    // Inheritable: no
    // Partial: no
    // =========================================================
    public enum Status
    {
        Pending,
        Active,
        Disabled
    }

    // =========================================================
    // 12. RECORD CLASS
    // Declarable: yes
    // Instantiable: yes
    // Inheritable: yes, unless sealed
    // =========================================================
    public record class PersonRecord(string Name, int Age);

    public record class EmployeeRecord(string Name, int Age, int Id)
        : PersonRecord(Name, Age);

    // =========================================================
    // 13. RECORD STRUCT
    // Declarable: yes
    // Instantiable: yes
    // Inheritable as base type: no
    // =========================================================
    public record struct Coordinate(int X, int Y);

    // =========================================================
    // 14. DELEGATE
    // Declarable: yes
    // Instantiable: yes, by assigning method/lambda
    // Inheritable: not used as normal class inheritance target
    // =========================================================
    public delegate int BinaryOp(int a, int b);

    // =========================================================
    // 15. NESTED TYPES
    // Nested types can be private/protected/internal/public/etc.
    // Top-level types cannot be private or protected.
    // =========================================================
    public class Container
    {
        private class NestedPrivateHelper
        {
            public void Help() => Console.WriteLine("Nested private helper");
        }

        protected class NestedProtectedHelper
        {
            public void Help() => Console.WriteLine("Nested protected helper");
        }

        public void UseHelper()
        {
            var h = new NestedPrivateHelper();
            h.Help();
        }
    }

    // INVALID TOP-LEVEL DECLARATIONS:
    // private class TopLevelPrivateClass { }
    // protected class TopLevelProtectedClass { }
    //
    // Reason:
    // Top-level types can generally be only public or internal.

    // =========================================================
    // 16. SAMPLE PROGRAM
    // =========================================================
    public static class StructuresRules
    {
        public static void Exec()
        {
            // Interface cannot be instantiated directly
            ILogger logger = new Dog();
            logger.Log("hello");

            // Abstract class cannot be instantiated directly
            Animal animal = new Dog();
            animal.Speak();
            animal.Sleep();

            // Normal class
            var dog = new Dog();
            dog.Speak();

            // Sealed class can be instantiated
            var util = new FinalUtility();
            util.Run();

            // Static class usage
            Console.WriteLine(MathHelper.Add(2, 3));

            // Internal class is usable here because same assembly context
            var worker = new InternalWorker();
            worker.Work();

            InternalTools.Ping();

            // Partial class
            var customer = new Customer { Name = "Santosh" };
            customer.Print();

            // Abstract partial base + derived concrete class
            Device router = new Router();
            router.Start();
            router.Reset();

            // Struct
            var p = new Point { X = 10, Y = 20 };
            p.Log("point created");

            // Enum
            Status s = Status.Active;
            Console.WriteLine(s);

            // Record class
            var p1 = new PersonRecord("Santosh", 50);
            var p2 = new PersonRecord("Santosh", 50);
            Console.WriteLine(p1 == p2); // True

            // Record struct
            var c = new Coordinate(1, 2);
            Console.WriteLine(c);

            // Delegate
            BinaryOp add = (a, b) => a + b;
            Console.WriteLine(add(5, 7));
        }
    }
}