namespace Refresher
{
    /*
    =========================================================================
    ACCESS SPECIFIER TABLE

    Top-level type access:
    - public    : accessible from any assembly that references this assembly
    - internal  : accessible only within the same assembly

    Member access:
    - public              : accessible from anywhere
    - private             : accessible only within the containing type
    - protected           : accessible within the containing type and derived types
    - internal            : accessible only within the same assembly
    - protected internal  : accessible from same assembly OR from derived types in other assemblies
    - private protected   : accessible within the containing type OR derived types in the same assembly only

    Quick matrix:

    | Specifier            | Same Class | Derived Class | Same Assembly | Other Assembly |
    |----------------------|------------|---------------|---------------|----------------|
    | public               | Yes        | Yes           | Yes           | Yes            |
    | private              | Yes        | No            | No            | No             |
    | protected            | Yes        | Yes           | No*           | No*            |
    | internal             | Yes        | Yes**         | Yes           | No             |
    | protected internal   | Yes        | Yes           | Yes           | Yes***         |
    | private protected    | Yes        | Yes****       | Yes****       | No             |

    Notes:
    * protected does not mean "same assembly"; it means "through inheritance"
    ** if visible through normal type/member rules in same assembly
    *** only for derived classes outside the assembly
    **** only for derived classes in the same assembly

    Top-level types can generally be only:
    - public
    - internal

    Top-level types cannot generally be:
    - private
    - protected
    - private protected
    - protected internal

    =========================================================================
    POLYMORPHISM KEYWORDS

    virtual
      - Declared in a base class
      - Says: this member may be overridden in a derived class

    override
      - Declared in a derived class
      - Says: I am replacing the inherited virtual/abstract/override implementation

    new
      - Declared in a derived class
      - Says: I am hiding a member from the base class
      - This is NOT true runtime polymorphic override

    Key rule:
    - A derived class can override only members that are virtual, abstract, or already override
    - If the base member is not virtual, then the derived member can only hide it with new

    =========================================================================
    EXPECTED OUTPUT / BEHAVIOR

    For virtual + override:
        Vehicle v2 = new Car();
        v2.StartEngine();   // Car engine starts with push button

    For new:
        Vehicle v3 = new Car();
        v3.Refuel();        // Vehicle refuels normally

        Car c2 = new Car();
        c2.Refuel();        // Car uses premium charging/refuel mode

    Why?
    - StartEngine() participates in runtime polymorphism because it is virtual/override
    - Refuel() does not, because Car.Refuel() hides Vehicle.Refuel() with new
    =========================================================================
    */

    // =========================================================
    // BASE CLASS
    // =========================================================
    public class Vehicle
    {
        // virtual:
        // Base class explicitly allows derived classes to replace this behavior.
        public virtual void StartEngine()
        {
            Console.WriteLine("Vehicle engine starts");
        }

        // non-virtual:
        // This method cannot be overridden in a derived class.
        // A derived class can only hide it with "new".
        public void Refuel()
        {
            Console.WriteLine("Vehicle refuels normally");
        }

        // -----------------------------------------------------
        // ACCESS MODIFIER EXAMPLES
        // -----------------------------------------------------

        // public:
        // Accessible from anywhere.
        public string PublicName = "PublicName";

        // private:
        // Accessible only inside Vehicle.
        private string PrivateCode = "PrivateCode";

        // protected:
        // Accessible inside Vehicle and in derived classes.
        protected string ProtectedTag = "ProtectedTag";

        // internal:
        // Accessible anywhere in the same assembly.
        internal string InternalId = "InternalId";

        // protected internal:
        // Accessible either from the same assembly OR from a derived class in another assembly.
        protected internal string ProtectedInternalNote = "ProtectedInternalNote";

        // private protected:
        // Accessible inside Vehicle and in derived classes, but only within the same assembly.
        private protected string PrivateProtectedKey = "PrivateProtectedKey";

        public void ShowOwnAccess()
        {
            // Inside the same class, all members are accessible.
            Console.WriteLine(PublicName);
            Console.WriteLine(PrivateCode);
            Console.WriteLine(ProtectedTag);
            Console.WriteLine(InternalId);
            Console.WriteLine(ProtectedInternalNote);
            Console.WriteLine(PrivateProtectedKey);
        }
    }

    // =========================================================
    // DERIVED CLASS
    // =========================================================
    public class Car : Vehicle
    {
        // override:
        // Replaces Vehicle.StartEngine() because the base method is virtual.
        // This is true runtime polymorphism.
        public override void StartEngine()
        {
            Console.WriteLine("Car engine starts with push button");
        }

        // new:
        // Hides Vehicle.Refuel() because Vehicle.Refuel() is not virtual.
        // This is compile-time member hiding, not polymorphic override.
        public new void Refuel()
        {
            Console.WriteLine("Car uses premium charging/refuel mode");
        }

        public void ShowDerivedAccess()
        {
            // Accessible in derived class:
            Console.WriteLine(PublicName);              // public
            Console.WriteLine(ProtectedTag);            // protected
            Console.WriteLine(InternalId);              // internal
            Console.WriteLine(ProtectedInternalNote);   // protected internal
            Console.WriteLine(PrivateProtectedKey);     // private protected (same assembly)

            // Not accessible:
            // Console.WriteLine(PrivateCode); // ERROR: private to Vehicle
        }
    }

    // =========================================================
    // NON-DERIVED CLASS IN SAME ASSEMBLY
    // =========================================================
    internal class GarageWorker
    {
        public void Inspect(Vehicle vehicle)
        {
            // Accessible here:
            Console.WriteLine(vehicle.PublicName);             // public
            Console.WriteLine(vehicle.InternalId);             // internal
            Console.WriteLine(vehicle.ProtectedInternalNote);  // protected internal via same assembly

            // Not accessible here because GarageWorker is not derived from Vehicle:
            // Console.WriteLine(vehicle.PrivateCode);         // ERROR: private
            // Console.WriteLine(vehicle.ProtectedTag);        // ERROR: protected
            // Console.WriteLine(vehicle.PrivateProtectedKey); // ERROR: private protected
        }
    }

    // =========================================================
    // NESTED CLASS EXAMPLE
    // =========================================================
    public class Outer
    {
        // Nested types may use modifiers like private/protected/public/internal.
        protected class NestedProtectedHelper
        {
            public void Run()
            {
                Console.WriteLine("Nested protected helper running");
            }
        }
    }

    // =========================================================
    // DEMO PROGRAM
    // =========================================================
    public class AccessAndPolymorphism
    {
        public static void Exec()
        {
            Console.WriteLine("=== virtual + override ===");

            Vehicle v1 = new Vehicle();
            Vehicle v2 = new Car();
            Car c1 = new Car();

            v1.StartEngine(); // Vehicle engine starts
            v2.StartEngine(); // Car engine starts with push button -> runtime polymorphism
            c1.StartEngine(); // Car engine starts with push button

            Console.WriteLine();
            Console.WriteLine("=== new (method hiding) ===");

            Vehicle v3 = new Car();
            Car c2 = new Car();

            v3.Refuel(); // Vehicle refuels normally
            c2.Refuel(); // Car uses premium charging/refuel mode

            /*
            Why the difference?

            StartEngine():
            - Vehicle.StartEngine() is virtual
            - Car.StartEngine() overrides it
            - Method dispatch uses the runtime object type
            - So Vehicle v2 = new Car(); v2.StartEngine(); calls Car.StartEngine()

            Refuel():
            - Vehicle.Refuel() is not virtual
            - Car.Refuel() uses new, so it hides the base member
            - Method resolution uses the compile-time reference type
            - So Vehicle v3 = new Car(); v3.Refuel(); calls Vehicle.Refuel()
            - But Car c2 = new Car(); c2.Refuel(); calls Car.Refuel()
            */

            Console.WriteLine();
            Console.WriteLine("=== access checks ===");

            Car car = new Car();
            car.ShowDerivedAccess();

            Vehicle vehicle = new Vehicle();
            vehicle.ShowOwnAccess();
        }
    }
}