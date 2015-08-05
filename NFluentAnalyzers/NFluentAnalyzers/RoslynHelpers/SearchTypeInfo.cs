namespace NFluentAnalyzers.RoslynHelpers
{
    using System;
    using Microsoft.CodeAnalysis;

    class SearchTypeInfo
    {
        public string Name { get; }

        public SearchTypeInfo(string name)
        {
            Name = name;
        }

        public bool Equals(ITypeSymbol typeSymbol)
        {
            if (typeSymbol == null)
            {
                return false;
            }

            return CSharpTypeNaming.GetTypeNameNoArity(typeSymbol).Equals(Name, StringComparison.Ordinal);
        }
    }
}