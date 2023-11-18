using Microsoft.CodeAnalysis.CodeFixes;

namespace Dzidek.Net.RoslynAnalyzers.TestsBase;

// ReSharper disable once ClassNeverInstantiated.Global
public class MultipleFilesCodeFixVerifier<TAnalyzer, TCodeFix> : CodeFixVerifier<TAnalyzer, TCodeFix, MultipleFilesCSharpCodeFixTest<TAnalyzer, TCodeFix>, XUnitVerifier>
  where TAnalyzer : DiagnosticAnalyzer, new()
  where TCodeFix : CodeFixProvider, new()
{
  // ReSharper disable once MemberCanBePrivate.Global
  public static Task VerifyCodeFixAsync(IEnumerable<string> sources, DiagnosticResult[] expected, IEnumerable<string> fixedSources)
  {
    var test = new MultipleFilesCSharpCodeFixTest<TAnalyzer, TCodeFix>
    {
      TestCodes = sources,
      FixedCodes = fixedSources,
    };

    test.ExpectedDiagnostics.AddRange(expected);
    return test.RunAsync(CancellationToken.None);
  }

  public static Task VerifyCodeFixAsync(IEnumerable<string> sources, DiagnosticResult expected, IEnumerable<string> fixedSources) =>
    VerifyCodeFixAsync(sources, new[]
    {
      expected
    }, fixedSources);
}

public class MultipleFilesCSharpCodeFixTest<TAnalyzer, TCodeFix> : CSharpCodeFixTest<TAnalyzer, TCodeFix, XUnitVerifier>
  where TAnalyzer : DiagnosticAnalyzer, new()
  where TCodeFix : CodeFixProvider, new()
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

  public IEnumerable<string> FixedCodes
  {
    set
    {
      foreach (var code in value)
      {
        FixedState.Sources.Add(code);
      }
    }
  }
}
