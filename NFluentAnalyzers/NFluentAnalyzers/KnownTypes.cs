namespace NFluentAnalyzers
{
	using JetBrains.Annotations;
	using Microsoft.CodeAnalysis;

	static class KnownTypes
	{
		[CanBeNull]
		public static INamedTypeSymbol GetNFluentCheck([NotNull] Compilation compilation)
		{
			return compilation.GetTypeByMetadataName("NFluent.Check");
		}

		[CanBeNull]
		public static INamedTypeSymbol GetNUnitAssert([NotNull] Compilation compilation)
		{
			return compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
		}
	}
}