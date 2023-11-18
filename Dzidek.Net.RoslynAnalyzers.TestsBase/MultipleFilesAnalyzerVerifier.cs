namespace Dzidek.Net.RoslynAnalyzers.TestsBase;

// ReSharper disable once ClassNeverInstantiated.Global
public class MultipleFilesAnalyzerVerifier<TAnalyzer> : AnalyzerVerifier<TAnalyzer, MultipleFilesCSharpAnalyzerTest<TAnalyzer, XUnitVerifier>, XUnitVerifier>
  where TAnalyzer : DiagnosticAnalyzer, new()
{
  public static Task VerifyAnalyzerAsync(IEnumerable<string> sources, params DiagnosticResult[] expected)
  {
    var test = new MultipleFilesCSharpAnalyzerTest<TAnalyzer, XUnitVerifier>
    {
      TestCodes = sources,
    };

    test.ExpectedDiagnostics.AddRange(expected);
    return test.RunAsync(CancellationToken.None);
  }
}
