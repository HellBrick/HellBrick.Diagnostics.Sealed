using HellBrick.Diagnostics.Assertions;
using Xunit;

namespace HellBrick.Diagnostics.Sealed.Test
{
	public class SealedAnalyzerTest
	{
		private readonly AnalyzerVerifier<SealedAnalyzer, SealedCodeFixProvider> _verifier
			= AnalyzerVerifier
			.UseAnalyzer<SealedAnalyzer>()
			.UseCodeFix<SealedCodeFixProvider>();

		[Fact]
		public void ClassWithNoDescendantsGetsSealed()
			=> _verifier
			.Source( "public class C {}" )
			.ShouldHaveFix( "public sealed class C {}" );

		[Fact]
		public void ClassWithDescendantDoesNotGetSealed()
			=> _verifier
			.Source
			(
@"
public class Base {}
public class Derived : Base {}
"
			)
			.ShouldHaveFix
			(
@"
public class Base {}
public sealed class Derived : Base {}
"
			);
	}
}
