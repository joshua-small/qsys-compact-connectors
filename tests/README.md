# Regression checks

The portable suite compiles the same `ConnectorSnapRules.cs` used by the
production adapters. It requires only .NET 10.0.112, runs on Windows or Linux,
and does not read Designer assemblies, preferences or designs:

```text
dotnet run --project tests/Portable/SnapRulesTests.csproj -c Release --artifacts-path artifacts/portable-tests
```

It covers signed and sub-unit pin/corner corrections, ties, absent targets,
independent axis decisions, open/closed body geometry and rejected non-finite
body heights or widths. The helpers preserve the existing rules; they do not
introduce new snapping tolerances or additional geometry validation.

This does not execute the reflection bindings, Harmony installation/cleanup,
native domains, endpoint guides, mixed-selection eligibility or Ctrl/Alt input
handling. Run the existing WPF suite on Windows for synthetic host-adapter
coverage:

```text
dotnet run --project tests/GeometryTests.csproj -c Release --artifacts-path artifacts/geometry-tests
```

Application acceptance for physical gestures and lifecycle integration remains
tracked in issue #5. A passing portable test is not a Designer or DSP acceptance
result. Any change to the production adapters also needs the WPF suite.
