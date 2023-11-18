# Roslyn Analyzers Boundaries

A set of three projects that includes Roslyn analyzers with code fix providers.

## Content
### Dzidek.Net.RoslynAnalyzers.Boundaries
A .NET Standard project with implementations of sample analyzers and code fix providers.
**You must build this project to see the results (warnings) in the IDE.**

- [InternalInterfaceBoundariesAnalyzer.cs](InternalInterfaceBoundaries%2FInternalInterfaceBoundariesAnalyzer.cs) Analyzer reporting incorrect use of internal interface
- [ChangeToPublicInterfaceCodeFixProvider.cs](InternalInterfaceBoundaries%2FChangeToPublicInterfaceCodeFixProvider.cs) A code fix changing internal modifier to public
- [RemoveInternalInterfaceCodeFixProvider.cs](InternalInterfaceBoundaries%2FRemoveInternalInterfaceCodeFixProvider.cs) A code fix removing parameter with object with internal modifier from other namespaces

### Dzidek.Net.RoslynAnalyzers.Boundaries.Sample
A project that references the sample analyzers. Note the parameters of `ProjectReference` in [Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.csproj](../Dzidek.Net.RoslynAnalyzers.Boundaries.Sample/Dzidek.Net.RoslynAnalyzers.Boundaries.Sample.csproj), they make sure that the project is referenced as a set of analyzers. 

### Dzidek.Net.RoslynAnalyzers.Boundaries.Tests
Unit tests for the sample analyzers and code fix provider. The easiest way to develop language-related features is to start with unit tests.

### Package
A project that builds a nuget package

## How To?
### How to debug?
- Use the [launchSettings.json](Properties/launchSettings.json) profile.
- Debug tests.

### How can I determine which syntax nodes I should expect?
Consider installing the Roslyn syntax tree viewer plugin [Rossynt](https://plugins.jetbrains.com/plugin/16902-rossynt/).

### Learn more about wiring analyzers
The complete set of information is available at [roslyn github repo wiki](https://github.com/dotnet/roslyn/blob/main/docs/wiki/README.md).