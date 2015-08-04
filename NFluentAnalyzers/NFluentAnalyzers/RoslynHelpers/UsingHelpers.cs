namespace NFluentAnalyzers.RoslynHelpers
{
	using System.Collections.Immutable;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.CodeAnalysis.Simplification;
	using NFluentAnalyzers.RoslynExtensions;
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	static class UsingHelpers
	{
		public static bool IsNamespaceInUsing(SemanticModel semanticModel, SyntaxNode forNode, INamespaceSymbol ns)
		{
			return semanticModel.GetUsingNamespacesInScope(forNode).Any(usingNamespace => usingNamespace.Equals(ns));
		}

		public static async Task<Document> AddUsingsAsync(Document document, SyntaxNode relativeToNode,
			CancellationToken cancellationToken, params INamespaceSymbol[] namespaces)
		{
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);

			var namespaceQualifiedStrings = namespaces
				.Select(ns => ns.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
				.ToImmutableHashSet();

			var toReallyAdd = GetNamespacesToReallyAdd(relativeToNode, semanticModel, namespaceQualifiedStrings);

			if (toReallyAdd.Count == 0)
			{
				return document;
			}
			
			var newRoot = await GetNewRootWithAddedNamespaces(document, relativeToNode, cancellationToken,
				namespaceQualifiedStrings);

			return document.WithSyntaxRoot(newRoot);
		}

		static async Task<SyntaxNode> GetNewRootWithAddedNamespaces(Document document, SyntaxNode relativeToNode,
			CancellationToken cancellationToken, ImmutableHashSet<string> namespaceQualifiedStrings)
		{
			var namespaceWithUsings = relativeToNode
				.GetAncestorsOrThis<NamespaceDeclarationSyntax>()
				.FirstOrDefault(ns => ns.DescendantNodes().OfType<UsingDirectiveSyntax>().Any());

			var root = await document.GetSyntaxRootAsync(cancellationToken);
			SyntaxNode newRoot;

			var usings = namespaceQualifiedStrings
				.Select(ns => UsingDirective(ParseName(ns).WithAdditionalAnnotations(Simplifier.Annotation)));

			if (namespaceWithUsings != null)
			{
				var newNamespaceDeclaration = namespaceWithUsings.WithUsings(namespaceWithUsings.Usings.AddRange(usings));
				newRoot = root.ReplaceNode(namespaceWithUsings, newNamespaceDeclaration);
			}
			else
			{
				var compilationUnit = (CompilationUnitSyntax)root;
				newRoot = compilationUnit.WithUsings(compilationUnit.Usings.AddRange(usings));
			}
			return newRoot;
		}

		static ImmutableList<string> GetNamespacesToReallyAdd(SyntaxNode relativeToNode, SemanticModel semanticModel,
			ImmutableHashSet<string> namespaceQualifiedStrings)
		{
			var alreadyInScope = semanticModel
				.GetUsingNamespacesInScope(relativeToNode)
				.Select(ns => ns.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat))
				.ToImmutableHashSet();

			var toReallyAdd = namespaceQualifiedStrings
				.Where(ns => !alreadyInScope.Contains(ns))
				.ToImmutableList();

			return toReallyAdd;
		}
	}
}