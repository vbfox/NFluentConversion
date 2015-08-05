namespace NFluentAnalyzers
{
    using System.Collections.Immutable;
    using NFluentAnalyzers.RoslynHelpers;

    static class NUnitAssertMethods
    {
        public static SearchTypeInfo Type = new SearchTypeInfo("NUnit.Framework.Assert");

        public static ImmutableList<SearchMethodInfo> AreEqual = ImmutableList<SearchMethodInfo>.Empty
            .Add(new SearchMethodInfo(Type, "AreEqual", new[] { "int", "int" }, isStatic: true))
            .Add(new SearchMethodInfo(Type, "AreEqual", new[] { "decimal", "decimal" }, isStatic: true))
            .Add(new SearchMethodInfo(Type, "AreEqual", new[] { "float", "float" }, isStatic: true))
            .Add(new SearchMethodInfo(Type, "AreEqual", new[] { "double", "double" }, isStatic: true))
            .Add(new SearchMethodInfo(Type, "AreEqual", new[] { "object", "object" }, isStatic: true));

        public static ImmutableList<SearchMethodInfo> AreNotEqual = ImmutableList<SearchMethodInfo>.Empty
            .Add(new SearchMethodInfo(Type, "AreNotEqual", new[] { "int", "int" }, isStatic: true))
            .Add(new SearchMethodInfo(Type, "AreNotEqual", new[] { "decimal", "decimal" }, isStatic: true))
            .Add(new SearchMethodInfo(Type, "AreNotEqual", new[] { "float", "float" }, isStatic: true))
            .Add(new SearchMethodInfo(Type, "AreNotEqual", new[] { "double", "double" }, isStatic: true))
            .Add(new SearchMethodInfo(Type, "AreNotEqual", new[] { "object", "object" }, isStatic: true));

        public static ImmutableList<SearchMethodInfo> All = ImmutableList<SearchMethodInfo>.Empty
            .AddRange(AreEqual)
            .AddRange(AreNotEqual);
    }
}
