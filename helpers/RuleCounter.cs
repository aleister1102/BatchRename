namespace BatchRename
{
    public class RuleCounter
    {
        private int _counter = 0;
        private static RuleCounter _instance;

        private RuleCounter()
        { }

        public static RuleCounter GetInstance()
        {
            if (_instance is null)
                _instance = new RuleCounter();
            return _instance;
        }

        public void Increment() => _counter++;

        internal void Decrement() => _counter--;

        public int GetValue() => _counter;

        public void Reset() => _counter = 0;
    }
}