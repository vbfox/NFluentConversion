namespace NFluentAnalyzers
{
    using System.Collections.Immutable;
    using Microsoft.CodeAnalysis;
    using Microsoft.CodeAnalysis.CSharp;
    using Microsoft.CodeAnalysis.CSharp.Syntax;
    using Microsoft.CodeAnalysis.Diagnostics;
    using NFluentAnalyzers.Helpers;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ReplaceNUnitAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "ReplaceNUnitAnalyzer";

        private static readonly DiagnosticDescriptor descriptor = new DiagnosticDescriptor(
			DiagnosticId,
			"Replace with NFluent",
			"Replace {0} with NFluent",
			"Naming",
			DiagnosticSeverity.Info,
			isEnabledByDefault: true,
            customTags: EmptyArray.Of<string>(),
            description: "Replace NUnit Assert with the equivalent one in NFluent");

        static readonly ImmutableArray<DiagnosticDescriptor> supportedDiagnostics = ImmutableArray.Create(descriptor);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => supportedDiagnostics;

        public override void Initialize(AnalysisContext context)
        {
            context.RegisterCompilationStartAction(CompilationStart);
        }

        static readonly SyntaxKind[] wantedKinds = { SyntaxKind.InvocationExpression };

        void CompilationStart(CompilationStartAnalysisContext context)
        {
            var nfluentCheck = context.Compilation.NFluentCheckType();
            if (nfluentCheck == null) return;

            var nunitAssert = context.Compilation.NUnitAssertType();
            if (nunitAssert == null) return;

            var nunitAssertForClosure = nunitAssert;
            context.RegisterSyntaxNodeAction(c => AnalyzeInvocation(c, nunitAssertForClosure), wantedKinds);
        }

        private static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, INamedTypeSymbol nunitAssert)
        {
            var invocation = (InvocationExpressionSyntax)context.Node;
            if (!(invocation.Expression is MemberAccessExpressionSyntax))
            {
                return;
            }

            var memberAccess = (MemberAccessExpressionSyntax)invocation.Expression;
            if (!CouldBeAnAssert(memberAccess))
            {
                return;
            }

            var symbol = context.SemanticModel.GetSymbolInfo(memberAccess).Symbol as IMethodSymbol;
            AnalyzeInvocation(context, symbol, invocation, nunitAssert);
        }

        static void AnalyzeInvocation(SyntaxNodeAnalysisContext context, IMethodSymbol symbol,
            InvocationExpressionSyntax invocation, INamedTypeSymbol nunitAssert)
        {
            if (symbol?.MethodKind != MethodKind.Ordinary
                || !symbol.IsStatic
                || !NUnitAssertMethods.Type.Equals(symbol.ContainingType)) return;

            if (!symbol.ContainingType.Equals(nunitAssert)) return;

            if (!IsAnAssert(symbol)) return;

            var diagnostic = Diagnostic.Create(descriptor, invocation.GetLocation(), symbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        static bool IsAnAssert(IMethodSymbol symbol)
        {
            for (var i = 0; i < NUnitAssertMethods.All.Count; i++)
            {
                if (NUnitAssertMethods.All[i].Equals(symbol, false))
                {
                    return true;
                }
            }
            return false;
        }

        static bool CouldBeAnAssert(MemberAccessExpressionSyntax memberAccess)
        {
            for (var i = 0; i < NUnitAssertMethods.All.Count; i++)
            {
                if (NUnitAssertMethods.All[i].CouldBeEqualto(memberAccess))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
