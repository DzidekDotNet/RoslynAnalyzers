namespace Dzidek.Net.RoslynAnalyzers.TestsBase;

public class MultipleFilesCSharpAnalyzerTest<TAnalyzer, TVerifier> : CSharpAnalyzerTest<TAnalyzer, TVerifier>
  where TAnalyzer : DiagnosticAnalyzer, new()
  where TVerifier : IVerifier, new()
{
  public IEnumerable<string> TestCodes
  {
    set
    {
      foreach (var code in value)
      {
        TestState.Sources.Add(code);
      }
    }
  }
}
