
# {Rule violation message}

|||
|:-|:-|
| Id | **{Rule Id}**
| Mnemo | {Rule mnemo}
| Severity | {⛔, ⚠ or ℹ}  {Rule severity: Failure, Warning or Hint}
| Category | [{Rule group name}](./Group_{Rule group file name}.md)[,.. additional group and category links]
| Source code | [{Rule source file name}.cs](../../../Rules/{Rule subfolder}/{Rule source file name}.cs)

## Cause

{Description of the cause for the rule to be triggered, for example: This rule is triggered if cross-database reference not via synonyms detected.}

<p id="descr">{A short recommendations on what to do to fix the violation found. This text will be displayed as a tooltip in the VS interface. Please note that this element cannot contain nested HTML or Markdown markup elements. Example: Use synonyms for cross-database reference.}</p>

## Examples

Bad

```sql
{T-SQL code example with violation
It supposed to be minimally necessary to show the error. No need for lengthy constructions. No need to separate batches with GO. If you are really struggle to find out examples, you can take a look here TSQLLinter\TeamTools.TSQL.LinterTests\UnitTests
}
```

Good

```sql
{T-SQL code example fixed}
```

## Tips

⚠️ {One or more important points to note.}

💡 { One or more additional helpful comments }

## Links

{ Links to official MS SQL Server docs or related rule doc files }
