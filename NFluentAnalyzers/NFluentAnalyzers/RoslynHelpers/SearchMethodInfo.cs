namespace NFluentAnalyzers.RoslynHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Linq;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp.Syntax;

    class SearchMethodInfo
    {
        public SearchTypeInfo Type { get; }
        public string MethodName { get; }
        public ImmutableList<string> Parameters { get; }
        public ImmutableList<string> TypeParameters { get; }
        public bool? IsStatic { get; }
        public bool? IsAbstract { get; }

        public SearchMethodInfo(SearchTypeInfo type, string methodName, IEnumerable<string> parameters = null,
            IEnumerable<string> typeParameters = null, bool? isStatic = null, bool? isAbstract = null)
        {
            Type = type;
            MethodName = methodName;
            Parameters = parameters?.ToImmutableList();
            TypeParameters = typeParameters == null ? ImmutableList<string>.Empty : (typeParameters.ToImmutableList());
            IsStatic = isStatic;
            IsAbstract = isAbstract;
        }

        public bool CouldBeEqualto(MemberAccessExpressionSyntax memberAccess)
        {
            if (memberAccess == null) return false;
            return memberAccess.Name.Identifier.Text == MethodName;
        }

        private static bool IsType(ITypeSymbol actual, string expected)
        {
            return CSharpTypeNaming.GetTypeNameNoArity(actual).Equals(expected, StringComparison.Ordinal);
        }

        public bool Equals(IMethodSymbol method, bool compareType = true)
        {
            if (method == null || method.MethodKind != MethodKind.Ordinary || method.Name != MethodName)
            {
                return false;
            }

            if (compareType && !Type.Equals(method.ContainingType))
            {
                return false;
            }

            if (IsStatic != null && method.IsStatic != IsStatic)
            {
                return false;
            }

            if (IsAbstract != null && method.IsAbstract != IsAbstract)
            {
                return false;
            }

            if (Parameters != null)
            {
                if (Parameters.Count != method.Parameters.Length)
                {
                    return false;
                }
                var parametersMatching = Parameters.Zip(
                    method.Parameters,
                    (expected, actual) => IsType(actual.Type, expected));

                if (parametersMatching.Any(ok => !ok))
                {
                    return false;
                }
            }

            if (TypeParameters.Count != method.Arity)
            {
                return false;
            }

            var typeParametersMatching = method.TypeArguments.Zip(TypeParameters, IsType);
            if (typeParametersMatching.Any(ok => !ok))
            {
                return false;
            }

            return true;
        }
    }
}
