using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace HellBrick.Diagnostics.Sealed
{
	[ExportCodeFixProvider( LanguageNames.CSharp, Name = nameof( SealedCodeFixProvider ) ), Shared]
	public class SealedCodeFixProvider : CodeFixProvider
	{
		public override ImmutableArray<string> FixableDiagnosticIds { get; } = ImmutableArray.Create( SealedAnalyzer.DiagnosticId );
		public override FixAllProvider GetFixAllProvider() => WellKnownFixAllProviders.BatchFixer;

		public override Task RegisterCodeFixesAsync( CodeFixContext context )
		{
			CodeAction codeFix = CodeAction.Create( "Seal the class", ct => SealClassAsync( ct ) );
			context.RegisterCodeFix( codeFix, context.Diagnostics[ 0 ] );
			return Task.CompletedTask;

			async Task<Document> SealClassAsync( CancellationToken cancellationToken )
			{
				SyntaxNode root = await context.Document.GetSyntaxRootAsync( cancellationToken ).ConfigureAwait( false );
				ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax) root.FindNode( context.Span );

				ClassDeclarationSyntax newDeclaration = classDeclaration.AddModifiers( SyntaxFactory.Token( SyntaxKind.SealedKeyword ) );
				SyntaxNode newRoot = root.ReplaceNode( classDeclaration, newDeclaration );
				return context.Document.WithSyntaxRoot( newRoot );
			}
		}
	}
}
