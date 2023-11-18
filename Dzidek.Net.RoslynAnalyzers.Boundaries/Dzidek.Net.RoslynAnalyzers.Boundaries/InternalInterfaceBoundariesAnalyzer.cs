using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Dzidek.Net.RoslynAnalyzers.Boundaries;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class InternalInterfaceBoundariesAnalyzer : DiagnosticAnalyzer
{
  internal const string DiagnosticId = "DDN0001";

  private readonly static LocalizableString Title = new LocalizableResourceString(nameof(Resources.DDN0001Title),
    Resources.ResourceManager, typeof(Resources));

  private readonly static LocalizableString MessageFormat =
    new LocalizableResourceString(nameof(Resources.DDN0001MessageFormat), Resources.ResourceManager, typeof(Resources));

  private readonly static LocalizableString Description =
    new LocalizableResourceString(nameof(Resources.DDN0001Description), Resources.ResourceManager, typeof(Resources));

  private const string Category = "Design";

  private readonly static DiagnosticDescriptor Rule = new(DiagnosticId, Title, MessageFormat, Category,
    DiagnosticSeverity.Warning, isEnabledByDefault: true, description: Description);

  public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } =
    ImmutableArray.Create(Rule);

  public override void Initialize(AnalysisContext context)
  {
    context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
    context.EnableConcurrentExecution();
    context.RegisterSyntaxNodeAction(AnalyzeConstructorDeclaration, SyntaxKind.ConstructorDeclaration);
  }

  private void AnalyzeConstructorDeclaration(SyntaxNodeAnalysisContext context)
  {
    var constructorDeclaration = (ConstructorDeclarationSyntax)context.Node;
    foreach (var parameter in constructorDeclaration.ParameterList.Parameters)
    {
      var parameterTypeSymbol = context.SemanticModel.GetTypeInfo(parameter.Type).Type;
      if (parameterTypeSymbol is not ({ TypeKind: TypeKind.Interface or TypeKind.Class } and ISymbol { DeclaredAccessibility: Accessibility.Internal }))
        continue;
      
      var classNamespace = context.ContainingSymbol.ContainingNamespace.ToDisplayString();
      var ns = parameterTypeSymbol as INamedTypeSymbol;
      var interfaceNamespace = ns.ContainingNamespace.ToDisplayString();
      if (interfaceNamespace.StartsWith(classNamespace))
        continue;
      
      var diagnostic = Diagnostic.Create(Rule,
        parameter.GetLocation(),
        parameterTypeSymbol.Name);
        
      context.ReportDiagnostic(diagnostic);
    }
  }
}
