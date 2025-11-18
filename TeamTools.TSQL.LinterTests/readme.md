
# Особенности оформления тестов

Subfolders under UnitTests folder are used for test organization only - namespaces should not be expanded in a per-folder manner.

By default tests are using 150 compatibility level parser. To run tests using different compatibility level use command as shown below:

```cmd
dotnet test TeamTools.TSQL.LinterTests -- 'TestRunParameters.Parameter(name=\"CompatibilityLevel\", value=\"100\")'
```
