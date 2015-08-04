namespace NFluentAnalyzers.RoslynExtensions
{
	using System.Collections.Generic;
	using System.Linq;
	using JetBrains.Annotations;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp.Syntax;

	static class SyntaxNodeExtensions
	{
		[CanBeNull]
		public static TAncestor GetAncestor<TAncestor>([CanBeNull] this SyntaxNode syntaxNode)
			where TAncestor : SyntaxNode
		{
			return syntaxNode.GetAncestors<TAncestor>().FirstOrDefault();
		}

		[NotNull]
		public static IEnumerable<T> GetAncestors<T>([CanBeNull] this SyntaxNode syntaxNode) where T : SyntaxNode
		{
			return syntaxNode == null
				? Enumerable.Empty<T>()
				: syntaxNode.Ancestors().OfType<T>();
		}

		[CanBeNull]
		public static T GetAncestorOrThis<T>([CanBeNull] this SyntaxNode syntaxNode) where T : SyntaxNode
		{
			return GetAncestorsOrThis<T>(syntaxNode).FirstOrDefault();
		}

		[NotNull]
		public static IEnumerable<T> GetAncestorsOrThis<T>([CanBeNull] this SyntaxNode syntaxNode) where T : SyntaxNode
		{
			return syntaxNode == null
				? Enumerable.Empty<T>()
				: syntaxNode.AncestorsAndSelf().OfType<T>();
		}

		public static bool AreUsingsContainedInNamespace([CanBeNull] this SyntaxNode contextNode)
		{
			return contextNode
				?.GetAncestor<NamespaceDeclarationSyntax>()
				?.DescendantNodes()
				.OfType<UsingDirectiveSyntax>()
				.FirstOrDefault() != null;
		}

		/// <summary>
		/// Returns the list of using directives that affect <paramref name="node"/>. The list will be returned in
		/// top down order.  
		/// </summary>
		public static IEnumerable<UsingDirectiveSyntax> GetEnclosingUsingDirectives([NotNull] this SyntaxNode node)
		{
			var inCompilationUnit = node.GetAncestorOrThis<CompilationUnitSyntax>()?.Usings
			                        ?? Enumerable.Empty<UsingDirectiveSyntax>();

			var inNamespaceDeclarations = node
				.GetAncestorsOrThis<NamespaceDeclarationSyntax>()
				.Reverse()
				.SelectMany(n => n.Usings);

            return inCompilationUnit.Concat(inNamespaceDeclarations);
		}
	}
}