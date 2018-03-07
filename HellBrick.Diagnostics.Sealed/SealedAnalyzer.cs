using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace HellBrick.Diagnostics.Sealed
{
	[DiagnosticAnalyzer( LanguageNames.CSharp )]
	public class SealedAnalyzer : DiagnosticAnalyzer
	{
		public const string DiagnosticId = "HBSealed";

		private static readonly DiagnosticDescriptor _rule
			= new DiagnosticDescriptor
			(
				DiagnosticId,
				"Class should be sealed",
				"Class '{0}' should be sealed",
				"Design",
				DiagnosticSeverity.Warning,
				isEnabledByDefault: true
			);

		public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create( _rule );

		public override void Initialize( AnalysisContext context )
		{
			context.ConfigureGeneratedCodeAnalysis( GeneratedCodeAnalysisFlags.None );
			context.RegisterCompilationStartAction( c => OnCompilationStart( c ) );

			void OnCompilationStart( CompilationStartAnalysisContext compilationStartContext )
			{
				HashSet<ClassDeclarationSyntax> unsealedClassDeclarations = new HashSet<ClassDeclarationSyntax>();
				HashSet<ClassDeclarationSyntax> usedBaseClassDeclarations = new HashSet<ClassDeclarationSyntax>();

				compilationStartContext.RegisterSyntaxNodeAction( c => OnClassDeclarationSyntaxNode( c ), SyntaxKind.ClassDeclaration );
				compilationStartContext.RegisterCompilationEndAction( c => OnCompilationEnded( c ) );

				void OnClassDeclarationSyntaxNode( SyntaxNodeAnalysisContext classNodeContext )
				{
					ClassDeclarationSyntax classDeclaration = (ClassDeclarationSyntax) classNodeContext.Node;

					if ( !classDeclaration.Modifiers.Any( SyntaxKind.SealedKeyword ) )
						unsealedClassDeclarations.Add( classDeclaration );

					foreach ( BaseTypeSyntax baseNode in classDeclaration.BaseList?.Types ?? default )
					{
						TypeInfo typeInfo = classNodeContext.SemanticModel.GetTypeInfo( baseNode.Type );
						if ( typeInfo.Type.TypeKind == TypeKind.Class )
						{
							foreach ( ClassDeclarationSyntax baseClassDeclaration in typeInfo.Type.DeclaringSyntaxReferences.Select( sr => sr.GetSyntax() ).OfType<ClassDeclarationSyntax>() )
								usedBaseClassDeclarations.Add( baseClassDeclaration );
						}
					}
				}

				void OnCompilationEnded( CompilationAnalysisContext compilationEndedContext )
				{
					unsealedClassDeclarations.ExceptWith( usedBaseClassDeclarations );

					foreach ( ClassDeclarationSyntax unsealedClassDeclaration in unsealedClassDeclarations )
					{
						Location modifierListLocation = Location.Create( unsealedClassDeclaration.SyntaxTree, unsealedClassDeclaration.Modifiers.Span );
						Diagnostic diagnostic = Diagnostic.Create( _rule, modifierListLocation, unsealedClassDeclaration.Identifier.Text );
						compilationEndedContext.ReportDiagnostic( diagnostic );
					}
				}
			}
		}
	}
}
