using System.Collections.Immutable;
using System.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FindSymbols;
using Microsoft.CodeAnalysis.Rename;

namespace Dzidek.Net.RoslynAnalyzers.Boundaries;

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

        // Register a code action that will invoke the fix.
        context.RegisterCodeFix(
            CodeAction.Create(
                title: string.Format(Resources.DDN0001CodeFixTitle_Remove, declaration.Identifier.Text),
                createChangedDocument: c => RemoveInternalInterfaceParameterAsync(context.Document, declaration, c),
                equivalenceKey: nameof(Resources.DDN0001CodeFixTitle_Remove)),
            diagnostic);
    }

    /// <summary>
    /// Executed on the quick fix action raised by the user.
    /// </summary>
    /// <param name="document">Affected source file.</param>
    /// <param name="parameterSyntax">Highlighted parameter declaration Syntax Node.</param>
    /// <param name="cancellationToken">Any fix is cancellable by the user, so we should support the cancellation token.</param>
    /// <returns>Clone of the solution with updates: renamed class.</returns>
    private async Task<Document> RemoveInternalInterfaceParameterAsync(
        Document document,
        ParameterSyntax parameterSyntax, 
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        return document.ReplaceSyntaxRoot(root?.RemoveNode(parameterSyntax, SyntaxRemoveOptions.KeepNoTrivia));
    } 
}

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
        
        context.RegisterCodeFix(
            CodeAction.Create(
                title: "XXX",
                createChangedSolution: c => ChangeToPublicAsync(context.Document, declaration, c),
                equivalenceKey: "XXX"),
            diagnostic);
    }
    
    private async Task<Solution> ChangeToPublicAsync(
        Document document,
        ParameterSyntax parameterSyntax, 
        CancellationToken cancellationToken)
    {
        var root = await document.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var semanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        var paramSymbol = semanticModel.GetDeclaredSymbol(parameterSyntax);
        var type = paramSymbol.Type;
        // root.FindNode()
        var project = document.Project;
        // project.Documents.Where(x => x.)
        var references = await SymbolFinder.FindReferencesAsync(paramSymbol, document.Project.Solution, cancellationToken).ConfigureAwait(false);
        // var newType = parameterSyntax.WithoutLeadingTrivia();
        var interfaceDocument = project.Documents.First(x => x.FilePath == type.DeclaringSyntaxReferences[0].SyntaxTree.FilePath);
        var interfaceDocumentRoot = await interfaceDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false);
        var interfaceDocumentSemanticModel = await document.GetSemanticModelAsync(cancellationToken).ConfigureAwait(false);
        // var interfaceDocumentTypeSymbol = semanticModel.GetDeclaredSymbol(type);
        var interfaceSyntax = await type.DeclaringSyntaxReferences[0].GetSyntaxAsync(cancellationToken).ConfigureAwait(false);
        // var newType = interfaceSyntax.WithoutLeadingTrivia().WithModifiers(modifiers);
        if (interfaceSyntax is InterfaceDeclarationSyntax interfaceDeclarationSyntax)
        {
            var newType = interfaceDeclarationSyntax.WithoutLeadingTrivia().WithModifiers(new SyntaxTokenList(SyntaxFactory.Token(SyntaxKind.PublicKeyword)));
            var newDocument = interfaceDocument.ReplaceSyntaxRoot(interfaceDocumentRoot.ReplaceNode(interfaceDeclarationSyntax, newType));
            // project = project.RemoveDocument(interfaceDocument.Id);
            // project.Solution.
            return project.Solution.WithDocumentSyntaxRoot(interfaceDocument.Id, await newDocument.GetSyntaxRootAsync(cancellationToken).ConfigureAwait(false));
        }
        
        return project.Solution;
    }
}