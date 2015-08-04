namespace NFluentAnalyzers.RoslynExtensions
{
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.CodeAnalysis.Simplification;

	static class NamedTypeSymbolExtensions
	{
		public static QualifiedNameSyntax ToNameSyntax(this INamedTypeSymbol typeSymbol)
		{
			return SyntaxFactory.QualifiedName(
				SyntaxFactory.ParseName(typeSymbol.ContainingNamespace.ToDisplayString()),
				SyntaxFactory.IdentifierName(typeSymbol.Name))
				.WithAdditionalAnnotations(Simplifier.Annotation);
		}
	}
}