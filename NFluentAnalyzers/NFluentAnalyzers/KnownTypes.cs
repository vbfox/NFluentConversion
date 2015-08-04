namespace NFluentAnalyzers
{
	using JetBrains.Annotations;
	using Microsoft.CodeAnalysis;

	static class KnownTypes
	{
		[CanBeNull]
		public static INamedTypeSymbol NFluentCheckType([NotNull] this Compilation compilation)
		{
			return compilation.GetTypeByMetadataName("NFluent.Check");
		}

		[CanBeNull]
		public static INamedTypeSymbol NUnitAssertType([NotNull] this Compilation compilation)
		{
			return compilation.GetTypeByMetadataName("NUnit.Framework.Assert");
		}
	}
}