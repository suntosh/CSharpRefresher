namespace ExceptionHandling
{
    /*
    =========================================================================
    DETAILED EXCEPTION HANDLING DEEP DIVE

    This file demonstrates:

    1. Basic try / catch / finally
    2. Multiple catch blocks
    3. Catch filters with "when"
    4. Nested try/catch blocks
    5. Custom exceptions
    6. Exception wrapping with inner exceptions
    7. Difference between:
         - throw;
         - throw ex;
    8. Rethrowing while preserving stack trace
    9. Domain/service/repository style layering
    10. Best practices

    =========================================================================
    CORE IDEAS

    try
    - put risky code here

    catch
    - handle a specific exception type

    finally
    - cleanup that must happen whether exception occurs or not

    throw;
    - rethrows the CURRENT exception
    - preserves original stack trace

    throw ex;
    - throws the caught exception object again
    - resets stack trace origin to this line
    - usually a mistake unless you explicitly want that behavior

    =========================================================================
    IMPORTANT RULES

    Rule 1:
    - Catch the most specific exception types you can handle meaningfully.

    Rule 2:
    - Do not swallow exceptions silently.

    Rule 3:
    - Use "throw;" when rethrowing from a catch block.

    Rule 4:
    - Wrap exceptions only when adding useful domain/context information.

    Rule 5:
    - Preserve the original exception as InnerException when wrapping.

    =========================================================================
    */

    // =========================================================
    // CUSTOM EXCEPTIONS
    // =========================================================

    /*
    A custom exception is useful when a domain has business-specific failure types.

    Example:
    - customer not found
    - payment declined
    - order validation failed
    */

    public class CustomerNotFoundException : Exception
    {
        public int CustomerId { get; }

        public CustomerNotFoundException(int customerId)
            : base($"Customer with ID {customerId} was not found.")
        {
            CustomerId = customerId;
        }

        public CustomerNotFoundException(int customerId, string message)
            : base(message)
        {
            CustomerId = customerId;
        }

        public CustomerNotFoundException(int customerId, string message, Exception innerException)
            : base(message, innerException)
        {
            CustomerId = customerId;
        }
    }

    public class OrderProcessingException : Exception
    {
        public int OrderId { get; }

        public OrderProcessingException(int orderId, string message)
            : base(message)
        {
            OrderId = orderId;
        }

        public OrderProcessingException(int orderId, string message, Exception innerException)
            : base(message, innerException)
        {
            OrderId = orderId;
        }
    }

    public class PaymentDeclinedException : Exception
    {
        public decimal Amount { get; }

        public PaymentDeclinedException(decimal amount, string message)
            : base(message)
        {
            Amount = amount;
        }
    }

    // =========================================================
    // DOMAIN MODELS
    // =========================================================

    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
    }

    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public decimal Amount { get; set; }
    }

    // =========================================================
    // REPOSITORY LAYER
    // =========================================================

    public class CustomerRepository
    {
        private readonly Dictionary<int, Customer> _customers = new()
        {
            { 1, new Customer { Id = 1, Name = "Santosh" } },
            { 2, new Customer { Id = 2, Name = "Alice" } }
        };

        public Customer GetCustomerById(int customerId)
        {
            /*
            Basic guard/validation:
            Throw an exception immediately when inputs violate method contract.
            */
            if (customerId <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(customerId),
                    "Customer ID must be greater than zero.");
            }

            if (!_customers.TryGetValue(customerId, out Customer? customer))
            {
                throw new CustomerNotFoundException(customerId);
            }

            return customer;
        }
    }

    public class PaymentGateway
    {
        public void Charge(decimal amount)
        {
            /*
            Simulated payment logic:
            - negative or zero amount: argument error
            - very high amount: domain-specific decline
            */

            if (amount <= 0)
            {
                throw new ArgumentException("Charge amount must be positive.", nameof(amount));
            }

            if (amount > 5000m)
            {
                throw new PaymentDeclinedException(amount, "Payment provider declined the transaction.");
            }

            Console.WriteLine($"[PAYMENT] Charged {amount:C}");
        }
    }

    // =========================================================
    // SERVICE LAYER
    // =========================================================

    public class OrderService
    {
        private readonly CustomerRepository _customerRepository;
        private readonly PaymentGateway _paymentGateway;

        public OrderService(CustomerRepository customerRepository, PaymentGateway paymentGateway)
        {
            _customerRepository = customerRepository;
            _paymentGateway = paymentGateway;
        }

        public void ProcessOrder(Order order)
        {
            /*
            This method demonstrates:
            - nested exception handling
            - multiple catch blocks
            - wrapping exceptions with inner exceptions
            */

            if (order == null)
            {
                throw new ArgumentNullException(nameof(order));
            }

            try
            {
                Console.WriteLine($"[SERVICE] Starting order {order.Id}");

                // Nested try/catch:
                // One operation may need its own local handling before we continue/abort.
                Customer customer;
                try
                {
                    customer = _customerRepository.GetCustomerById(order.CustomerId);
                    Console.WriteLine($"[SERVICE] Customer found: {customer.Name}");
                }
                catch (CustomerNotFoundException ex)
                {
                    Console.WriteLine($"[SERVICE] Local handling: customer lookup failed for ID {order.CustomerId}");

                    /*
                    Wrap with more business context:
                    - preserve original exception as InnerException
                    - now the upper layer gets an order-specific failure
                    */
                    throw new OrderProcessingException(
                        order.Id,
                        $"Order {order.Id} cannot be processed because customer {order.CustomerId} does not exist.",
                        ex);
                }

                // Another risky area:
                try
                {
                    _paymentGateway.Charge(order.Amount);
                }
                catch (PaymentDeclinedException ex)
                {
                    /*
                    Re-throwing with more context:
                    This adds order-level meaning while preserving original cause.
                    */
                    throw new OrderProcessingException(
                        order.Id,
                        $"Order {order.Id} payment failed for amount {order.Amount:C}.",
                        ex);
                }

                Console.WriteLine($"[SERVICE] Order {order.Id} processed successfully");
            }
            catch (OrderProcessingException)
            {
                /*
                This catch intentionally rethrows without changing stack trace.

                IMPORTANT:
                Use "throw;" not "throw ex;" when you only want to propagate upward.
                */
                Console.WriteLine($"[SERVICE] OrderProcessingException for order {order.Id}, rethrowing...");
                throw;
            }
            catch (ArgumentException ex)
            {
                /*
                Convert low-level argument-related failure into a service/domain failure
                only if that helps the caller reason about the operation.
                */
                throw new OrderProcessingException(
                    order.Id,
                    $"Order {order.Id} has invalid data.",
                    ex);
            }
            finally
            {
                /*
                finally always runs whether success or exception happens,
                unless the process is terminated abruptly.
                */
                Console.WriteLine($"[SERVICE] Final cleanup for order {order.Id}");
            }
        }
    }

    // =========================================================
    // THROW VS THROW EX DEMO
    // =========================================================

    public static class ThrowRethrowDemo
    {
        public static void DemonstrateThrow()
        {
            try
            {
                Level1();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DemonstrateThrow caught exception and rethrows with: throw;");
                Console.WriteLine();
                Console.WriteLine("---- STACK TRACE PRESERVED VERSION ----");

                try
                {
                    throw;
                }
                catch (Exception preserved)
                {
                    Console.WriteLine(preserved);
                }
            }
        }

        public static void DemonstrateThrowEx()
        {
            try
            {
                Level1();
            }
            catch (Exception ex)
            {
                Console.WriteLine("DemonstrateThrowEx caught exception and rethrows with: throw ex;");
                Console.WriteLine();
                Console.WriteLine("---- STACK TRACE RESET VERSION ----");

                try
                {
                    throw ex;
                }
                catch (Exception reset)
                {
                    Console.WriteLine(reset);
                }
            }
        }

        private static void Level1()
        {
            Level2();
        }

        private static void Level2()
        {
            Level3();
        }

        private static void Level3()
        {
            throw new InvalidOperationException("Original failure deep in Level3.");
        }
    }

    // =========================================================
    // EXCEPTION FILTERS
    // =========================================================

    public static class ExceptionFilterDemo
    {
        public static void Run()
        {
            Console.WriteLine("----- Exception Filter Demo -----");

            try
            {
                SimulateFileRead("missing.txt");
            }
            catch (IOException ex) when (ex.Message.Contains("missing"))
            {
                /*
                Catch filters let you catch only when a condition is true.
                This is cleaner than catching everything and branching inside.
                */
                Console.WriteLine("Handled missing-file case with catch filter.");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Generic IO handling: {ex.Message}");
            }

            Console.WriteLine();
        }

        private static void SimulateFileRead(string fileName)
        {
            throw new IOException($"The file '{fileName}' is missing.");
        }
    }

    // =========================================================
    // BASIC TRY/CATCH/FINALLY DEMO
    // =========================================================

    public static class BasicExceptionDemo
    {
        public static void Run()
        {
            Console.WriteLine("----- Basic try/catch/finally Demo -----");

            try
            {
                Console.WriteLine("Inside try block");
                int x = 10;
                int y = 0;

                // This will throw DivideByZeroException
                int z = x / y;

                Console.WriteLine(z);
            }
            catch (DivideByZeroException ex)
            {
                Console.WriteLine($"Caught divide by zero: {ex.Message}");
            }
            catch (Exception ex)
            {
                /*
                General catch should usually be later, after specific catches.
                */
                Console.WriteLine($"General catch: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("finally block executed");
            }

            Console.WriteLine();
        }
    }

    // =========================================================
    // NESTED TRY/CATCH DEMO
    // =========================================================

    public static class NestedExceptionDemo
    {
        public static void Run()
        {
            Console.WriteLine("----- Nested try/catch Demo -----");

            try
            {
                Console.WriteLine("Outer try started");

                try
                {
                    Console.WriteLine("Inner try started");
                    throw new InvalidOperationException("Failure inside inner block.");
                }
                catch (InvalidOperationException ex)
                {
                    Console.WriteLine($"Inner catch handled: {ex.Message}");

                    /*
                    We can decide to escalate upward after local handling.
                    */
                    throw;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Outer catch received: {ex.Message}");
            }
            finally
            {
                Console.WriteLine("Outer finally executed");
            }

            Console.WriteLine();
        }
    }

    // =========================================================
    // MAIN PROGRAM
    // =========================================================

    public class ExceptionsBrush
    {
        public static void Exec()
        {
            Console.WriteLine("=== Exception Handling Deep Dive ===");
            Console.WriteLine();

            BasicExceptionDemo.Run();
            NestedExceptionDemo.Run();
            ExceptionFilterDemo.Run();

            Console.WriteLine("----- Domain/Service/Custom Exception Demo -----");

            var repository = new CustomerRepository();
            var paymentGateway = new PaymentGateway();
            var orderService = new OrderService(repository, paymentGateway);

            var orders = new List<Order>
            {
                new Order { Id = 101, CustomerId = 1, Amount = 250m },   // success
                new Order { Id = 102, CustomerId = 999, Amount = 100m }, // customer not found
                new Order { Id = 103, CustomerId = 2, Amount = 6000m },  // payment declined
                new Order { Id = 104, CustomerId = 2, Amount = -1m }     // invalid argument
            };

            foreach (var order in orders)
            {
                Console.WriteLine($"Processing order {order.Id}");

                try
                {
                    orderService.ProcessOrder(order);
                }
                catch (OrderProcessingException ex)
                {
                    /*
                    This is where higher layers often inspect:
                    - the exception message
                    - the custom properties
                    - the InnerException chain
                    */
                    Console.WriteLine($"[MAIN] OrderProcessingException caught: {ex.Message}");

                    if (ex.InnerException != null)
                    {
                        Console.WriteLine($"[MAIN] Inner exception type: {ex.InnerException.GetType().Name}");
                        Console.WriteLine($"[MAIN] Inner exception message: {ex.InnerException.Message}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[MAIN] Unexpected exception: {ex}");
                }

                Console.WriteLine();
            }

            Console.WriteLine("----- throw; vs throw ex; Demo -----");
            Console.WriteLine();

            ThrowRethrowDemo.DemonstrateThrow();
            Console.WriteLine();
            ThrowRethrowDemo.DemonstrateThrowEx();

            Console.WriteLine();
            Console.WriteLine("=== End of exception demo ===");
        }
    }

    /*
    =========================================================================
    BEST PRACTICES SUMMARY

    1. Prefer specific catches:
       Good:
           catch (FileNotFoundException ex) { ... }

       Less useful:
           catch (Exception ex) { ... }

    2. Use finally for cleanup:
       - file handles
       - connections
       - locks
       - temporary state reset

    3. Use custom exceptions for meaningful domain failures:
       - CustomerNotFoundException
       - OrderProcessingException
       - PaymentDeclinedException

    4. When wrapping:
       - add context
       - preserve original cause as InnerException

       Example:
           throw new OrderProcessingException("...", ex);

    5. Use "throw;" when rethrowing current exception:
       Good:
           catch (Exception)
           {
               throw;
           }

       Usually bad:
           catch (Exception ex)
           {
               throw ex;
           }

       Because "throw ex;" resets stack trace origin.

    6. Avoid swallowing exceptions:
       Bad:
           catch (Exception) { }

    7. Catch only what you can handle meaningfully:
       - recover
       - translate
       - log and propagate
       - cleanup and propagate

    =========================================================================
    */
}