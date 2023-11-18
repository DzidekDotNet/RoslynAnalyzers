using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Dzidek.Net.RoslynAnalyzers.Boundaries.InternalInterfaceBoundaries;

[ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(RemoveInternalInterfaceCodeFixProvider)), Shared]
public class RemoveInternalInterfaceCodeFixProvider : CodeFixProvider
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
    context.RegisterCodeFix(
      CodeAction.Create(
        title: string.Format(Resources.DDN0001CodeFixTitle_Remove, declaration.Identifier.Text),
        createChangedDocument: c => RemoveInternalInterfaceParameterAsync(context.Document, declaration, c),
        equivalenceKey: nameof(Resources.DDN0001CodeFixTitle_Remove)),
      diagnostic);
  }

  private async Task<Document> RemoveInternalInterfaceParameterAsync(
    Document document,
    ParameterSyntax parameterSyntax,
    CancellationToken cancellationToken)
  {
    var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
    return document.ReplaceSyntaxRoot(root?.RemoveNode(parameterSyntax, SyntaxRemoveOptions.KeepNoTrivia));
  }
}
