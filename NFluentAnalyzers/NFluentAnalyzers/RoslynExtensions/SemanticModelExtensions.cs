namespace NFluentAnalyzers.RoslynExtensions
{
	using System.Collections.Generic;
	using System.Collections.Immutable;
	using Microsoft.CodeAnalysis;

	static class SemanticModelExtensions
	{
		public static ISet<INamespaceSymbol> GetUsingNamespacesInScope(this SemanticModel semanticModel, SyntaxNode location)
		{
			// Avoiding linq here for perf reasons. This is used heavily in the AddImport service
			HashSet<INamespaceSymbol> result = null;

			foreach (var @using in location.GetEnclosingUsingDirectives())
			{
				if (@using.Alias == null)
				{
					var symbolInfo = semanticModel.GetSymbolInfo(@using.Name);
					if (symbolInfo.Symbol != null && symbolInfo.Symbol.Kind == SymbolKind.Namespace)
					{
						result = result ?? new HashSet<INamespaceSymbol>();
						result.Add((INamespaceSymbol)symbolInfo.Symbol);
					}
				}
			}

			return (ISet<INamespaceSymbol>)result ?? ImmutableHashSet<INamespaceSymbol>.Empty;
		}
	}
}
