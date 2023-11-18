using Microsoft.CodeAnalysis;

namespace Dzidek.Net.RoslynAnalyzers.Boundaries;

public static class DocumentExtensions
{
  public static Document ReplaceSyntaxRoot(this Document document, SyntaxNode? newRoot) => 
    newRoot == null ? document : document.WithSyntaxRoot(newRoot);
}
