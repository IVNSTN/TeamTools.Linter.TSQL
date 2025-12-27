
# Testing management considerations

Subfolders under UnitTests folder are used for test organization only - namespaces should not be expanded in a per-folder manner.

SQL test case file name must end with `_raise_N_violations.sql` pattern where `N` should be replaced with expected number of violations a being tested rule must detect
in given test case file. If no violations are expected then `N` is `0`, e.g. `all_good_raise_0_violations.sql`

## Different compatibility levels

By default tests are using **150** compatibility level parser. To run tests using different compatibility level use command as shown below:

```cmd
dotnet test TeamTools.TSQL.LinterTests -- 'TestRunParameters.Parameter(name=\"CompatibilityLevel\", value=\"100\")'
```

In some terminals quote escaping is not required:

```cmd
dotnet test TeamTools.TSQL.LinterTests -- 'TestRunParameters.Parameter(name="CompatibilityLevel", value="100")'
```

In `launch.json` for **VS Code** do not use surrounding quotes:

```json
    "program": "dotnet",
    "args": [
        "test",
        // ...
        "--",
        "TestRunParameters.Parameter(name=\"CompatibilityLevel\", value=\"100\")"
    ]
```

To limit test case source file minimal required compatibility level use this comment pattern in the very beginning of the file:

```sql
-- compatibility level min: 130
DROP TABLE IF EXISTS #tbl;
```

If `TestRunParameters.Parameter` option has provided `CompatibilityLevel` < 130 then the test case above is skipped accordingly to NUnit `Assume` directive behavior.
Test cases with modern T-SQL syntax might fail if parsed with older compatibility level.
