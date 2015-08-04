namespace NFluentAnalyzers
{
	using System.Collections.Immutable;
	using System.Composition;
	using System.Linq;
	using System.Threading;
	using System.Threading.Tasks;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CodeActions;
	using Microsoft.CodeAnalysis.CodeFixes;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using NFluentAnalyzers.RoslynExtensions;
	using NFluentAnalyzers.RoslynHelpers;
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ReplaceNUnitCodeFix)), Shared]
    public class ReplaceNUnitCodeFix : CodeFixProvider
	{
		public const string EQUIVALENCE_KEY = "ReplaceNUnitCodeFix";

		private static readonly ImmutableArray<string> fixableDiagnosticIds
			= ImmutableArray.Create(ReplaceNUnitAnalyzer.DIAGNOSTIC_ID);

		public override sealed ImmutableArray<string> FixableDiagnosticIds => fixableDiagnosticIds;

        public sealed override FixAllProvider GetFixAllProvider()
        {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context)
        {
            var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

	        foreach (var diagnostic in context.Diagnostics)
	        {
				var diagnosticSpan = diagnostic.Location.SourceSpan;

				var declaration = root
					.FindToken(diagnosticSpan.Start)
					.Parent
					.AncestorsAndSelf()
					.OfType<InvocationExpressionSyntax>().First();

				context.RegisterCodeFix(
					CodeAction.Create(
						title: "Replace with NFluent",
                        createChangedDocument: c => MakeUppercaseAsync(context.Document, declaration, c),
						equivalenceKey: EQUIVALENCE_KEY),
					diagnostic);
			}
        }

		static InvocationExpressionSyntax CheckThat(INamedTypeSymbol nfluentCheck, ArgumentSyntax argument)
	    {
		    return InvocationExpression(
			    MemberAccessExpression(
				    SyntaxKind.SimpleMemberAccessExpression,
				    nfluentCheck.ToNameSyntax(),
				    IdentifierName("That")),
			    ArgumentList(SingletonSeparatedList(argument)));
	    }

		async Task<Document> MakeUppercaseAsync(Document document, InvocationExpressionSyntax invocation,
			CancellationToken cancellationToken)
		{
			var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
			var typeSymbol = semanticModel.GetSymbolInfo(invocation.Expression, cancellationToken).Symbol;
			var nfluentCheck = semanticModel.Compilation.NFluentCheckType();

			if (typeSymbol == null || nfluentCheck == null) return document;

			var newMethod = typeSymbol.Name == "AreEqual" ? "IsEqualTo" : "IsNotEqualTo";

			var checkThat = CheckThat(nfluentCheck, invocation.ArgumentList.Arguments[1]);

			var replacement = InvocationExpression(
				MemberAccessExpression(
					SyntaxKind.SimpleMemberAccessExpression,
					checkThat,
					IdentifierName(newMethod)),
				ArgumentList(SingletonSeparatedList(invocation.ArgumentList.Arguments[0])))
				.WithTriviaFrom(invocation);

			var syntaxRoot = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
			var newSyntaxRoot = syntaxRoot.ReplaceNode(invocation, replacement);
			var newDocument = document.WithSyntaxRoot(newSyntaxRoot);

			var withUsings = await UsingHelpers.AddUsingsAsync(newDocument, replacement, cancellationToken,
				nfluentCheck.ContainingNamespace);

			return withUsings;
		}
	}
}