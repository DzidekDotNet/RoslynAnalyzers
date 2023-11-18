using System.Collections.Generic;
using Verifier = Dzidek.Net.RoslynAnalyzers.TestsBase.MultipleFilesAnalyzerVerifier<
  Dzidek.Net.RoslynAnalyzers.Boundaries.InternalInterfaceBoundariesAnalyzer>;

namespace Dzidek.Net.RoslynAnalyzers.Boundaries.Tests;

public class InternalInterfaceBoundariesAnalyzerTests
{
  [Fact]
  public async Task InternalInterfaceBoundariesAnalyzer_WhenClassInOtherNamespaceUsesInternalInterface_ShouldAlertDiagnostic()
  {
    var codes = new List<string>()
    {
      "using Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.NamespaceX;" +
      "" +
      "namespace Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.NamespaceY;" +
      "" +
      "internal class Y" +
      "{" +
      "  public Y(IXPublic xPublic, IXInternal xInternal)" +
      "  {" +
      "  }" +
      "}",

      "namespace Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.NamespaceX;" +
      "" +
      "" +
      "public interface IXPublic" +
      "{" +
      "}" +
      "" +
      "" +
      "internal interface IXInternal" +
      "{" +
      "}"
    };
    var expected = Verifier.Diagnostic()
      .WithSpan(1, 175, 1, 195)
      .WithArguments("IXInternal");
    await Verifier.VerifyAnalyzerAsync(codes, expected);
  }

  [Fact]
  public async Task InternalInterfaceBoundariesAnalyzer_WhenClassHigherInHierarchyUsesInternalInterface_ShouldNotAlertDiagnostic()
  {
    var codes = new List<string>()
    {
      "namespace Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.NamespaceX;" +
      "" +
      "" +
      "public interface IXPublic" +
      "{" +
      "}" +
      "" +
      "" +
      "internal interface IXInternal" +
      "{" +
      "}",

      "using Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.NamespaceX;" +
      "" +
      "namespace Dzidek.Net.RoslynAnalyzers.Boundaries.Sample;" +
      "" +
      "internal class Main" +
      "" +
      "{" +
      "  public Main(IXPublic xPublic, IXInternal xInternal)" +
      "  {" +
      "    " +
      "  }" +
      "}"
    };
    await Verifier.VerifyAnalyzerAsync(codes);
  }
}
