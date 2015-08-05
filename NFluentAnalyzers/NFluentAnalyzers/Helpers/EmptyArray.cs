namespace NFluentAnalyzers.Helpers
{
    static class EmptyArray
    {
        static class EmptyArrayStorage<T>
        {
            public static readonly T[] Value = new T[0];
        }

        public static T[] Of<T>()
        {
            return EmptyArrayStorage<T>.Value;
        }
    }
}