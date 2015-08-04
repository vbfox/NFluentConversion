namespace NFluentAnalyzers.RoslynExtensions
{
	using Microsoft.CodeAnalysis.CSharp.Syntax;
	using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

	static class InvocationExpressionSyntaxExtensions
	{
		public static InvocationExpressionSyntax WithArguments(this InvocationExpressionSyntax syntax, params ArgumentSyntax[] arguments)
		{
			return syntax.WithArgumentList(ArgumentList(SeparatedList(arguments)));
		}

		public static InvocationExpressionSyntax WithSingleArgument(this InvocationExpressionSyntax syntax, ArgumentSyntax argument)
		{
			return syntax.WithArgumentList(ArgumentList(SingletonSeparatedList(argument)));
		}
	}
}