
# TSQL Linter rules documentation

Linter supports compatibility levels from 100 till 160. Required compatibility level should be set in configuration file.

## Rule categories

|||
|:-|:-|
| **[FA*](./Category_Failure.md)** |Failure
| **[VU*](./Category_Vulnerability.md)** |Vulnerability
| **[DE*](./Category_Deprecation.md)** |Deprecation
| **[AM*](./Category_Ambiguity.md)** |Ambiguity
| **[RD*](./Category_Redundancy.md)** |Redundancy
| **[PF*](./Category_Performance.md)** |Performance
| **[MA*](./Category_Maintainability.md)** |Maintainability
| **[HD*](./Category_Hardcode.md)** |Hardcode
| **[NM*](./Category_Naming.md)** |Naming
| **[FM*](./Category_Formatting.md)** |Formatting
| **[CV*](./Category_CodingConvention.md)** |Coding Convention
| **[SI*](./Category_Simplification.md)** |Simplification
| **[DD*](./Category_DatabaseDesign.md)** |Database Design
| **[DM*](./Category_DatabaseMaintenance.md)** |Database Maintenance
| **[CD*](./Category_ContinuousDeployment.md)** |Continuous Deployment
| **[FL*](./Category_Files.md)** |Files
| **[CS*](./Category_CodeSmell.md)** |Code Smell

## Rule groups

These groups combine related rules across all categories:

* [Indices](./Group_Indices.md)
* [Cursors](./Group_Cursors.md)
* [Triggers](./Group_Triggers.md)
* [InMemory](./Group_InMemory.md)

---

## How to contribute

You can improve rule docs by editing markdown files. Help in text translation is welcome.
All changes should be reviewed in PR.

For a rule documentation file use [this template](./_rule_template.md) and

* Keep the docs file markdown as simple as possible
* If have no Tips or Links or Exmaples - don't add those block to the docs file, they can be added later
* All text within curly braces should be replaced with the actual documentation text (with no braces)
* Don't put HTML or markdown elements into `<p id="descr">` element text - this text will be displayed in rule violation tooltip window in Visual Studio, unfortunately it supports plain text only
* A single blank like before the very first header is required for valid rendering in Bitbucket
* Abstract from a specific company or agreements of the type "we decided so". Rules are described in formal language with an indication of the reason for the rule triggering and recommendations for elimination.
* Abbreviations such as *sp*, *proc*, *func* and similar jargon are not used.
* When updating main rule violation message, corresponding line in [ViolationMessages](../../../Resources/ViolationMessages.json) and in related docs Group or Category index file should be updated as well
* For better understanding of how rule docs file should look like - see existing doc files.
