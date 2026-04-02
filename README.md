
# TeamTools Linter TSQL

[![Code Analysis Rules](https://gist.githubusercontent.com/IVNSTN/1d72e5e0425f231b2de94fd91cd5ccd9/raw/rules-count.svg)](./TeamTools.TSQL.Linter/Resources)
[![License MIT](https://gist.githubusercontent.com/IVNSTN/1d72e5e0425f231b2de94fd91cd5ccd9/raw/License-MIT-purple.svg)](./LICENSE)
[![Coverage](https://gist.githubusercontent.com/IVNSTN/1d72e5e0425f231b2de94fd91cd5ccd9/raw/code-coverage.svg)](https://github.com/IVNSTN/TeamTools.Linter.TSQL/actions)

[\[English en-us\]](./README.md) [\[Русский ru-ru\]](./README.ru-ru.md)

A library for static analysis of T-SQL code.

## Rules

The library implements over 400 rules, grouped as follows:

- explicit errors
- potential vulnerabilities
- ambiguity and redundancy
- performance issues
- hard‑coded values
- naming conventions compliance
- formatting
- questionable code (code smells)
- and others

The documentation also provides rule grouping by specific database functionality:
indexes, triggers, cursors, in‑memory development.

[Rules documentation](./TeamTools.TSQL.Linter/Resources/Docs/en-us/readme.md)

## Installation

The library must be referenced in the configuration file of the command‑line utility [IVNSTN/TeamTools.Linter.CommandLine](https://github.com/IVNSTN/TeamTools.Linter.CommandLine) as one of the plugins.

This library is already included in the CLI utility distribution and is enabled by default. To start using the implemented rules, download the latest version of the CLI utility from the repository linked above.

## Configuration

💡 _For a trial run, use the config [EvaluateConfig.json](./TeamTools.TSQL.Linter/EvaluateConfig.json), which disables most rules related to formatting, naming, and similar conventions. Specify the path to this config in the console runner settings, or simply replace `DefaultConfig.json` with this file._

The plugin is configured by editing the [configuration file](./TeamTools.TSQL.Linter/DefaultConfig.json). You can create multiple config files—for example, one for linting with naming and formatting rules, another for detecting explicit and potential issues only.

Rules can be enabled or disabled, and their severity levels can be adjusted by setting the desired value next to the rule ID in the `rules` section of the config file:

| Value | Meaning |
| :-- | :-- |
| **off**     | 🚫 Rule is disabled |
| **hint**    | ℹ️ Rule violation is treated as an info message, suggestion, or recommendation |
| **warning** | ⚠️ Rule violation indicates a potentially significant warning, but not an explicit error |
| **error**   | ⛔ Explicit compilation or runtime error |


However, avoid overstating the importance of certain rules by setting their severity to `error` for violations of conventions or optimization suggestions. This could unnecessarily fail CI pipelines. Instead, adjust the console utility’s overall **sensitivity level** (e.g., `--severity warning`). See the utility’s documentation for details.

The required **compatibility level** for correct source code parsing must be set in the `options` section of the config file.

The `deprecation` section of the config file can be extended: specify the full name (including schema) of a function, stored procedure, or user‑defined data type as a key, and the linter will notify developers that the module should no longer be used. As the value, provide an explanation with a recommendation on what to replace the deprecated module call with.

To exclude a file from checks for one or more rules, add its name to the `whitelist` section of the config. In this section’s key, you can specify either the full file name (without path) or a wildcard pattern (e.g., `test*.test*.sql`, where `*` matches zero, one, or multiple characters).

## Integration

The library is designed to be plugged into the command‑line linting utility [IVNSTN/TeamTools.Linter.CommandLine](https://github.com/IVNSTN/TeamTools.Linter.CommandLine).

## Compatibility

Code parsing is performed by the [Microsoft/SqlScriptDOM](https://github.com/microsoft/SqlScriptDOM) library. The rules support MS SQL Server compatibility levels **100 to 170** inclusive.

Some rules may still work with other compatibility levels, but this has not been specifically tested.

## ⚠️ Known issues

| Rule Id | Issue | Description |
| :-- | :-- | :-- |
| **[CS0197](./TeamTools.TSQL.Linter/Resources/Docs/en-us/CS0197.md):CURSOR_COMMAND_ORDER**       | False-positive  | When cursor command is prepended with check of CURSOR_STATUS function then it’s fine no matter if command order looks like mistake. Rule implementation does not follow all code-flow branches and does not check IF-ELSE conditions. |
| **[CS0521](./TeamTools.TSQL.Linter/Resources/Docs/en-us/CS0521.md):SYSPROC_RETURN_NOT_CHECKED** | False-positive  | If `sp_` or `xp_` proc which has no RETURN code is not mentioned in ignore list then it might be falsely reported by this rule. |
| **[CS0920](./TeamTools.TSQL.Linter/Resources/Docs/en-us/CS0920.md):UNPAIRED_TRAN_STATEMENT**    | False-positive  | False-positive detection: TRAN control may be fine because of IF-ELSE, TRY-CATCH logic. Rule implementation does not follow all code-flow branches and does not check IF-ELSE conditions. |
| **[CS0921](./TeamTools.TSQL.Linter/Resources/Docs/en-us/CS0921.md):UNPAIRED_XMLDOC_STATEMENT**  | False-positive  | False-positive detection: XML doc control may be fine because of IF-ELSE, TRY-CATCH logic. Rule implementation does not follow all code-flow branches and does not check IF-ELSE conditions. |
| **[PF0929](./TeamTools.TSQL.Linter/Resources/Docs/en-us/PF0929.md):NON_SARGABLE_PREDICATE**     | False-positive  | Complex predicates may contain a _primary_ filter which leads to fine execution plan alongside with _minor_ filter considered as non-sargable predicate which has no negative effect on query performance. |
| **[FA0904](./TeamTools.TSQL.Linter/Resources/Docs/en-us/FA0904.md):INDEX_REFERS_UNKNOWN_COL**   | False-positive  | If a table name is reused along the script with different structure then unnecessary "missing column" warnings are shown. |
| **[FA0949](./TeamTools.TSQL.Linter/Resources/Docs/en-us/FA0949.md):COLUMN_NOT_IN_GROUP_BY**     | False-positive  | If a similar in it's essence expression is written in different ways in SELECT and GROUP BY clauses it may be reporter as not grouped. |
| **[RD0236](./TeamTools.TSQL.Linter/Resources/Docs/en-us/RD0236.md):REDUNDANT_NEWLINE**          | Low performance | Poor implementation leads to a substantial slowdown in the analysis process. |
| **[CD0215](./TeamTools.TSQL.Linter/Resources/Docs/en-us/CD0215.md):COMPUTED_COLS_ORDER**        | Controversial   | Regarding PERSISTED-colums the rule is right and not: such a column is anyways _computed_ ALTER, and at the same time is _stored_ thus moving the column towards column list tail means data reload. |

## Acknowledgments

Initially, the library was developed as a plugin for the [tsqllint](https://github.com/tsqllint/tsqllint) linter. Over time, its functionality outgrew that product’s capabilities, leading to its evolution into a standalone tool with its own runner and plugin protocol. Nevertheless, the team expresses deep gratitude to the authors of the mentioned project.
