

namespace Refresher
{
 
    public static class Basics
    {
        // Replace the incorrect field declaration with a valid const field declaration
        private const byte SLEEP = 100;
        private const byte STOP_WHILE = 4;

        public static void Exec()
        {
            Console.WriteLine("Hello, World!");
            for (int i = 0; i < 5; i++)
            {
                Console.WriteLine($"Iteration {i}");
            }
            //bool condition = true;

            int j = 0;
            do
            {
                j += 1;
                Thread.Sleep(SLEEP);
                Console.WriteLine($"This is a do-while loop {j}.");
            } while (j < STOP_WHILE);

           
            checked
            {
                int a = int.MaxValue;
                int b = 1;
                try
                {
                    int c = a + b; // This will throw an OverflowException
                    Console.WriteLine($"The result of {a} + {b} is {c}");
                }
                catch (OverflowException ex)
                {
                    Console.WriteLine("Overflow occurred: " + ex.Message);
                }
            }
        }
    }
}
