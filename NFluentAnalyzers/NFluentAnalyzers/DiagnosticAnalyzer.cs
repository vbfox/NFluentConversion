using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace NFluentAnalyzers
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NFluentAnalyzersAnalyzer : DiagnosticAnalyzer
    {
        public const string DiagnosticId = "NFluentAnalyzers";

        // You can change these strings in the Resources.resx file. If you do not want your analyzer to be localize-able, you can use regular strings for Title and MessageFormat.
        private static readonly LocalizableString Title = new LocalizableResourceString(nameof(Resources.AnalyzerTitle), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString MessageFormat = new LocalizableResourceString(nameof(Resources.AnalyzerMessageFormat), Resources.ResourceManager, typeof(Resources));
        private static readonly LocalizableString Description = new LocalizableResourceString(nameof(Resources.AnalyzerDescription), Resources.ResourceManager, typeof(Resources));
        private const string Category = "Naming";

        private static DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticId, Title, MessageFormat, Category, DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get { return ImmutableArray.Create(Rule); } }

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

            var nfluentCheck = context.SemanticModel.Compilation.GetTypeByMetadataName("NFluent.Check");
            if (nfluentCheck == null)
            {
                return;
            }

            var nunitAssert = context.SemanticModel.Compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
            if (nunitAssert == null)
            {
                return;
            }

            if (!symbol.ContainingType.Equals(nunitAssert))
            {
                return;
            }

            var diagnostic = Diagnostic.Create(Rule, memberAccess.GetLocation(), symbol.Name);

            context.ReportDiagnostic(diagnostic);
        }
    }
}
