# NFluent Conversion

[NFluent](https://github.com/tpierrain/NFluent) is a really nice library but
converting [NUnit](http://www.nunit.org/) code by hand is tedious.

This project contains refactorings and code fixes as a
[Roslyn](https://github.com/dotnet/roslyn)-based Visual Studio extension to
automate this task and transform NUnit asserts :

```csharp
Assert.AreEqual(expected, actual);
```

with NFluent ones :

```csharp
Check.That(actual).IsEqualTo(expected);
```
