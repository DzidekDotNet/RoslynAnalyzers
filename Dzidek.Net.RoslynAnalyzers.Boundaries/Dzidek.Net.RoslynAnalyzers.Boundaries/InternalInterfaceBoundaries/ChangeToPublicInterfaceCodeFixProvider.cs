using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dzidek.Net.RoslynAnalyzers.Boundaries.InternalInterfaceBoundaries;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(ChangeToPublicInterfaceCodeFixProvider)), Shared]
public class ChangeToPublicInterfaceCodeFixProvider : CodeFixProvider
{
  public sealed override ImmutableArray<string> FixableDiagnosticIds { get; } =
    ImmutableArray.Create(InternalInterfaceBoundariesAnalyzer.DiagnosticId);

  public override FixAllProvider? GetFixAllProvider() => null;

  public async sealed override Task RegisterCodeFixesAsync(CodeFixContext context)
  {
    var diagnostic = context.Diagnostics.Single();
    var diagnosticSpan = diagnostic.Location.SourceSpan;
    var root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);
    var diagnosticNode = root?.FindNode(diagnosticSpan);
    if (diagnosticNode is not ParameterSyntax declaration)
      return;
    
    var semanticModel = await context.Document.GetSemanticModelAsync(context.CancellationToken).ConfigureAwait(false);
    var paramSymbol = semanticModel.GetDeclaredSymbol(declaration);

    var paramType = paramSymbol?.Type;
    if(paramType is not ISymbol interfaceSymbol)
      return;
    
    if(context.Document.Project.Documents.All(x => x.FilePath != interfaceSymbol.DeclaringSyntaxReferences[0].SyntaxTree.FilePath))
      return;

    context.RegisterCodeFix(
      CodeAction.Create(
        title: string.Format(Resources.DDN0001CodeFixTitle_Change, interfaceSymbol.Name),
        createChangedSolution: c => ChangeInterfaceToPublicAsync(context.Document, interfaceSymbol, c),
        equivalenceKey: nameof(Resources.DDN0001CodeFixTitle_Change)),
      diagnostic);
  }

  private async Task<Solution> ChangeInterfaceToPublicAsync(
    Document document,
    ISymbol interfaceSymbol,
    CancellationToken cancellationToken)
  {
    var interfaceDocument = document.Project.Documents.First(x => x.FilePath == interfaceSymbol.DeclaringSyntaxReferences[0].SyntaxTree.FilePath);
    var interfaceDocumentRoot = await interfaceDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    if (interfaceDocumentRoot is null)
      return document.Project.Solution;
    
    var interfaceSyntax = await interfaceSymbol.DeclaringSyntaxReferences[0].GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
    if (interfaceSyntax is not InterfaceDeclarationSyntax interfaceDeclarationSyntax)
      return document.Project.Solution;
    
    var newType = interfaceDeclarationSyntax.WithoutLeadingTrivia().WithModifiers(new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
    var newDocument = interfaceDocument.ReplaceSyntaxRoot(interfaceDocumentRoot.ReplaceNode(interfaceDeclarationSyntax, newType));
    var newDocumentSyntax = await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    return newDocumentSyntax is null ? document.Project.Solution : document.Project.Solution.WithDocumentSyntaxRoot(interfaceDocument.Id, newDocumentSyntax);
  }
}
