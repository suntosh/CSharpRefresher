namespace Refresher
{
    public class Pipeline<T>
    {
        private readonly List<Func<T, T>> _steps = new();

        public void AddStep(Func<T, T> step)
        {
            _steps.Add(step);
        }

        public T Execute(T input)
        {
            T current = input;

            foreach (var step in _steps)
            {
                current = step(current);
            }

            return current;
        }
    }

    public class FuncAction
    {
        public static void Exec()
        {
            var pipeline = new Pipeline<string>();

            pipeline.AddStep(s => s.Trim());
            pipeline.AddStep(s => s.ToUpper());
            pipeline.AddStep(s => $"[{s}]");

            string result = pipeline.Execute("  santosh  ");
            Console.WriteLine(result); // [SANTOSH]
        }
    }
}
