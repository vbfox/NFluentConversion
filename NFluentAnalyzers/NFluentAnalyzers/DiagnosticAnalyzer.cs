namespace NFluentAnalyzers
{
	using System.Collections.Immutable;
	using Microsoft.CodeAnalysis;
	using Microsoft.CodeAnalysis.CSharp;
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using Microsoft.CodeAnalysis.Diagnostics;

	[DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NFluentAnalyzersAnalyzer : DiagnosticAnalyzer
    {
        public const string DIAGNOSTIC_ID = "NFluentAnalyzers";

        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
			DIAGNOSTIC_ID,
			"Replace with NFluent",
			"Replace {0} with NFluent",
			"Naming",
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
			description: "Replace NUnit Assert with the equivalent one in NFluent");

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(descriptor); } }

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterSyntaxNodeAction(AnalyzeInvocation, SyntaxKind.SimpleMemberAccessExpression);
        }

        private static readonly ImmutableHashSet<string> assertMethods = new[] {"AreEqual"}.ToImmutableHashSet();

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context)
        {
            var memberAccess = (MemberAccessExpressionSyntax)context.Node;

            if (!assertMethods.Contains(memberAccess.Name.Identifier.Text))
            {
                return;// Fast path
            }

            var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
            if (symbol == null)
            {
                return;
            }

            if (symbol.ContainingType.Name != "Assert"
                || !assertMethods.Contains(symbol.Name)
                || symbol.MethodKind != MethodKind.Ordinary
                || !symbol.IsStatic)
            {
                return;
            }

            var nfluentCheck = KnownTypes.GetNFluentCheck(context.SemanticModel.Compilation);
            if (nfluentCheck == null)
            {
                return;
            }

            var nunitAssert = KnownTypes.GetNUnitAssert(context.SemanticModel.Compilation);
            if (nunitAssert == null)
            {
                return;
            }

            if (!symbol.ContainingType.Equals(nunitAssert))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(descriptor, memberAccess.GetLocation(), symbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
