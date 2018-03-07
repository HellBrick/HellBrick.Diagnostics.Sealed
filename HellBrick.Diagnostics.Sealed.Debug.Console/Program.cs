using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.MSBuild;

namespace HellBrick.Diagnostics.Sealed.Debug.Console
{
	internal class Program
	{
		private static async Task Main( string[] args )
		{
			string solutionPath = args[ 0 ];

			MSBuildWorkspace workspace = MSBuildWorkspace.Create();
			Solution solution = await workspace.OpenSolutionAsync( solutionPath ).ConfigureAwait( false );

			SealedAnalyzer analyzer = new SealedAnalyzer();
			ImmutableArray<DiagnosticAnalyzer> analyzers = ImmutableArray.Create<DiagnosticAnalyzer>( analyzer );

			foreach ( Project project in solution.Projects )
			{
				Compilation compilation = await project.GetCompilationAsync().ConfigureAwait( false );
				CompilationWithAnalyzers compilationWithAnalyzers = compilation.WithAnalyzers( analyzers );
				ImmutableArray<Diagnostic> analyzerDiagnostics = await compilationWithAnalyzers.GetAnalyzerDiagnosticsAsync().ConfigureAwait( false );

				PrintDiagnostics( analyzerDiagnostics );

				void PrintDiagnostics( ImmutableArray<Diagnostic> diagnostics )
				{
					if ( diagnostics.Length > 0 )
					{
						System.Console.WriteLine( "----------------------------------------" );
						System.Console.WriteLine( $"{project.Name}: {diagnostics.Length} diagnostics" );
						foreach ( Diagnostic diagnostic in diagnostics )
						{
							System.Console.Write( "\t" );
							System.Console.WriteLine( diagnostic );
						}
					}
				}
			}
		}
	}
}
