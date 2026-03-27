

namespace DelegatePatternDemo
{
    // =========================================================
    // DOMAIN MODEL
    // =========================================================
    public class Order
    {
        public int Id { get; set; }
        public string Customer { get; set; } = "";
        public decimal Amount { get; set; }
        public bool IsPriority { get; set; }
        public bool IsInternational { get; set; }
    }

    // =========================================================
    // CUSTOM DELEGATES
    // =========================================================

    // Returns true/false for validation rules
    public delegate bool OrderValidationRule(Order order);

    // Returns a transformed amount
    public delegate decimal PricingRule(Order order, decimal currentPrice);

    // Event delegate
    public delegate void OrderProcessedHandler(object sender, OrderProcessedEventArgs e);

    // =========================================================
    // EVENT ARGS
    // =========================================================
    public class OrderProcessedEventArgs : EventArgs
    {
        public OrderProcessedEventArgs(Order order, decimal finalPrice, DateTime processedAt)
        {
            Order = order;
            FinalPrice = finalPrice;
            ProcessedAt = processedAt;
        }

        public Order Order { get; }
        public decimal FinalPrice { get; }
        public DateTime ProcessedAt { get; }
    }

    // =========================================================
    // ORDER PROCESSOR
    // =========================================================
    public class OrderProcessor
    {
        private readonly List<OrderValidationRule> _validationRules = new();
        private readonly List<PricingRule> _pricingRules = new();

        // Event: external code can subscribe/unsubscribe,
        // but only this class can raise it.
        public event OrderProcessedHandler? OrderProcessed;

        // Another event using built-in EventHandler<T>
        public event EventHandler<string>? ProcessingFailed;

        public void AddValidationRule(OrderValidationRule rule)
        {
            _validationRules.Add(rule);
        }

        public void AddPricingRule(PricingRule rule)
        {
            _pricingRules.Add(rule);
        }

        public decimal Process(Order order)
        {
            // 1. Run all validation delegates
            foreach (var rule in _validationRules)
            {
                bool ok = rule(order);
                if (!ok)
                {
                    OnProcessingFailed($"Order {order.Id} failed validation.");
                    throw new InvalidOperationException($"Order {order.Id} failed validation.");
                }
            }

            // 2. Apply pricing rules in sequence
            decimal finalPrice = order.Amount;

            foreach (var rule in _pricingRules)
            {
                finalPrice = rule(order, finalPrice);
            }

            // 3. Raise event after successful processing
            OnOrderProcessed(new OrderProcessedEventArgs(order, finalPrice, DateTime.UtcNow));

            return finalPrice;
        }

        protected virtual void OnOrderProcessed(OrderProcessedEventArgs e)
        {
            OrderProcessed?.Invoke(this, e);
        }

        protected virtual void OnProcessingFailed(string message)
        {
            ProcessingFailed?.Invoke(this, message);
        }
    }

    // =========================================================
    // SUBSCRIBERS
    // =========================================================
    public class AuditService
    {
        public void HandleOrderProcessed(object sender, OrderProcessedEventArgs e)
        {
            Console.WriteLine(
                $"[AUDIT] Order {e.Order.Id} processed for {e.Order.Customer}. Final Price = {e.FinalPrice:C}");
        }
    }

    public class EmailService
    {
        public void SendConfirmation(object sender, OrderProcessedEventArgs e)
        {
            Console.WriteLine(
                $"[EMAIL] Sent confirmation to {e.Order.Customer} for order {e.Order.Id}.");
        }
    }

    public class AnalyticsService
    {
        public void TrackOrder(object sender, OrderProcessedEventArgs e)
        {
            Console.WriteLine(
                $"[ANALYTICS] Tracking order {e.Order.Id}, priority={e.Order.IsPriority}, international={e.Order.IsInternational}");
        }
    }

    // =========================================================
    // PROGRAM
    // =========================================================
    public class DelegatesEvents
    {
        public static void Exec()
        {
            var processor = new OrderProcessor();

            // -------------------------------------------------
            // Add validation rules
            // -------------------------------------------------

            // Lambda delegate
            processor.AddValidationRule(order => !string.IsNullOrWhiteSpace(order.Customer));

            // Lambda delegate
            processor.AddValidationRule(order => order.Amount > 0);

            // Method group could also be used if desired
            processor.AddValidationRule(ValidateInternationalLimit);

            // -------------------------------------------------
            // Add pricing rules
            // -------------------------------------------------

            // Priority surcharge
            processor.AddPricingRule((order, price) =>
            {
                if (order.IsPriority)
                    return price + 25m;
                return price;
            });

            // International shipping surcharge
            processor.AddPricingRule((order, price) =>
            {
                if (order.IsInternational)
                    return price + 40m;
                return price;
            });

            // Discount rule
            processor.AddPricingRule((order, price) =>
            {
                if (order.Amount >= 500m)
                    return price * 0.90m; // 10% discount
                return price;
            });

            // Tax rule
            processor.AddPricingRule(AddTax);

            // -------------------------------------------------
            // Subscribe event handlers
            // -------------------------------------------------
            var audit = new AuditService();
            var email = new EmailService();
            var analytics = new AnalyticsService();

            processor.OrderProcessed += audit.HandleOrderProcessed;
            processor.OrderProcessed += email.SendConfirmation;
            processor.OrderProcessed += analytics.TrackOrder;

            processor.ProcessingFailed += (sender, message) =>
            {
                Console.WriteLine($"[ERROR] {message}");
            };

            // -------------------------------------------------
            // Process some orders
            // -------------------------------------------------
            var orders = new List<Order>
            {
                new Order
                {
                    Id = 1001,
                    Customer = "Alice",
                    Amount = 600m,
                    IsPriority = true,
                    IsInternational = false
                },
                new Order
                {
                    Id = 1002,
                    Customer = "Bob",
                    Amount = 200m,
                    IsPriority = false,
                    IsInternational = true
                },
                new Order
                {
                    Id = 1003,
                    Customer = "",
                    Amount = 100m,
                    IsPriority = false,
                    IsInternational = false
                }
            };

            foreach (var order in orders)
            {
                try
                {
                    decimal finalPrice = processor.Process(order);
                    Console.WriteLine($"[RESULT] Order {order.Id} final price = {finalPrice:C}");
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EXCEPTION] {ex.Message}");
                    Console.WriteLine();
                }
            }

            // -------------------------------------------------
            // Unsubscribe example
            // -------------------------------------------------
            processor.OrderProcessed -= analytics.TrackOrder;

            var anotherOrder = new Order
            {
                Id = 1004,
                Customer = "Charlie",
                Amount = 1000m,
                IsPriority = true,
                IsInternational = true
            };

            decimal final2 = processor.Process(anotherOrder);
            Console.WriteLine($"[RESULT] Order {anotherOrder.Id} final price = {final2:C}");
        }

        // -----------------------------------------------------
        // Named methods used as delegates
        // -----------------------------------------------------
        private static bool ValidateInternationalLimit(Order order)
        {
            if (order.IsInternational && order.Amount > 10000m)
                return false;

            return true;
        }

        private static decimal AddTax(Order order, decimal currentPrice)
        {
            return currentPrice * 1.08m; // 8% tax
        }
    }
}