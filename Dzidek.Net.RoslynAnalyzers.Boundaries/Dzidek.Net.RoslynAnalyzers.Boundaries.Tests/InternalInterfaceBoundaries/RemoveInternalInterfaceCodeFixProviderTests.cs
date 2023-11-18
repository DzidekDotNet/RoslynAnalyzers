using System.Collections.Generic;
using Dzidek.Net.RoslynAnalyzers.Boundaries.InternalInterfaceBoundaries;
using Dzidek.Net.RoslynAnalyzers.TestsBase;

namespace Dzidek.Net.RoslynAnalyzers.Boundaries.Tests.InternalInterfaceBoundaries;

using Verifier =
  MultipleFilesCodeFixVerifier<InternalInterfaceBoundariesAnalyzer,
    RemoveInternalInterfaceCodeFixProvider>;

public class RemoveInternalInterfaceCodeFixProviderTests
{
  [Fact]
  public async Task RemoveInternalInterfaceCodeFixProvider()
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

    var fixes = new List<string>()
    {
      "using Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.NamespaceX;" +
      "" +
      "namespace Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.NamespaceY;" +
      "" +
      "internal class Y" +
      "{" +
      "  public Y(IXPublic xPublic)" +
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
    await Verifier.VerifyCodeFixAsync(codes, expected, fixes).ConfigureAwait(false);
  }
}
