
# Rules of CodeSmell category

|||
|-|-|
| [CS0101](./CS0101.md) | SELECT-INTO is forbidden
| [CS0102](./CS0102.md) | Argument passed by position, not by name
| [CS0103](./CS0103.md) | Insert without column list
| [CS0105](./CS0105.md) | GOTO and labels are forbidden
| [CS0107](./CS0107.md) | Named constraint on temporary (#) table
| [CS0111](./CS0111.md) | Length or precision and scale should be defined for data type
| [CS0112](./CS0112.md) | Select * is forbidden
| [CS0118](./CS0118.md) | Not authorized schema creation
| [CS0125](./CS0125.md) | Unspecified grantor
| [CS0126](./CS0126.md) | Reading @@ROWCOUNT after NOCOUNT change
| [CS0128](./CS0128.md) | Index on different table in create table script
| [CS0134](./CS0134.md) | Forbidden RAISERROR option
| [CS0135](./CS0135.md) | Forbidden hint
| [CS0136](./CS0136.md) | READ UNCOMMITTED isolation level used
| [CS0138](./CS0138.md) | View with output containing SELECT *
| [CS0139](./CS0139.md) | Trigger ordering specified
| [CS0141](./CS0141.md) | GETDATE() is used for DATETIME2
| [CS0142](./CS0142.md) | Unspecified RETURN value
| [CS0143](./CS0143.md) | RETURN value not redefined explicitly after CATCH
| [CS0146](./CS0146.md) | SYSNAME is used for scalar variable
| [CS0147](./CS0147.md) | SYSNAME is used for table column
| [CS0151](./CS0151.md) | No COMMIT in CATCH expected
| [CS0152](./CS0152.md) | Redundant DISTINCT after GROUP BY
| [CS0159](./CS0159.md) | Non-ANSI NULL comparison
| [CS0160](./CS0160.md) | Output parameter never assigned
| [CS0161](./CS0161.md) | Unreachable code
| [CS0163](./CS0163.md) | Unexpected data fetching from a trigger
| [CS0164](./CS0164.md) | Permission misdirected
| [CS0171](./CS0171.md) | Fully qualified column instead of two-part links
| [CS0172](./CS0172.md) | This loop can never run more than once
| [CS0175](./CS0175.md) | Invalid error number value
| [CS0176](./CS0176.md) | Invalid error severity value
| [CS0177](./CS0177.md) | Invalid error state value
| [CS0181](./CS0181.md) | @@IDENTITY is used instead of SCOPE_IDENTITY()
| [CS0186](./CS0186.md) | RAISERROR is used instead of THROW in trigger
| [CS0187](./CS0187.md) | Transaction control attempt in trigger
| [CS0188](./CS0188.md) | Security context control in trigger
| [CS0189](./CS0189.md) | WAITFOR in triggers
| [CS0190](./CS0190.md) | Reading from empty table
| [CS0191](./CS0191.md) | Source table filled but never used
| [CS0195](./CS0195.md) | Cursor isn't declared as LOCAL
| [CS0196](./CS0196.md) | Cursor isn't declared explicitly as READONLY or FOR UPDATE
| [CS0197](./CS0197.md) | The order of cursor handling commands is incorrect
| [CS0198](./CS0198.md) | Trigger cyclical execution
| [CS0250](./CS0250.md) | Redundant unary operator
| [CS0266](./CS0266.md) | Table-level constraint is defined not at table level
| [CS0295](./CS0295.md) | ORDER BY uses column position
| [CS0299](./CS0299.md) | IIF includes nested conditional constructs or queries
| [CS0514](./CS0514.md) | Debug construction is used in code
| [CS0520](./CS0520.md) | App-lock is used in code
| [CS0521](./CS0521.md) | System procedure return value isn't checked
| [CS0522](./CS0522.md) | Logic of CONTEXT_INFO()
| [CS0523](./CS0523.md) | Unexpected code encryption
| [CS0524](./CS0524.md) | Unexpected data encryption
| [CS0710](./CS0710.md) | Arithmetics with NULL returns NULL
| [CS0723](./CS0723.md) | Alias is the same but column position does not match
| [CS0732](./CS0732.md) | The GO command is missing at the end of the script
| [CS0737](./CS0737.md) | Enabling the XACT_ABORT option within the trigger body
| [CS0739](./CS0739.md) | Schema needs name
| [CS0743](./CS0743.md) | Using non-invariant functions and subqueries inside CASE
| [CS0745](./CS0745.md) | Set-based operations are preferred over cursors for update
| [CS0746](./CS0746.md) | Unexpected date format control
| [CS0747](./CS0747.md) | Unexpected general set option control
| [CS0748](./CS0748.md) | IDENTITY_INSERT is used
| [CS0751](./CS0751.md) | Full text search is used
| [CS0756](./CS0756.md) | DEADLOCK_PRIORITY is initialized as numeric
| [CS0776](./CS0776.md) | SPARSE columns is used in temp tables or table variables
| [CS0781](./CS0781.md) | INFORMATION_SCHEMA views is mixed with SQL SERVER sys-views
| [CS0786](./CS0786.md) | Undocumented LINENO directive
| [CS0787](./CS0787.md) | ROWVERSION usage on temporary object
| [CS0789](./CS0789.md) | Approximate dataset usage
| [CS0790](./CS0790.md) | External dataset access
| [CS0790](./CS0791.md) | Unicode symbol lost in operation
| [CS0790](./CS0972.md) | Non-unicode literal contains Unicode symbol
| [CS0793](./CS0793.md) | String literal contain non-printable characters
| [CS0790](./CS0794.md) | Number has no unicode
| [CS0804](./CS0804.md) | Per-row deletion via cursor
| [CS0806](./CS0806.md) | IDENTITY_INSERT is used on temporary table
| [CS0807](./CS0807.md) | Global cursor reference
| [CS0808](./CS0808.md) | Redundant DISTINCT for IN predicate source
| [CS0821](./CS0821.md) | DECIMAL without scale
| [CS0830](./CS0830.md) | Same temporary table names
| [CS0832](./CS0832.md) | Table variable inside a function
| [CS0790](./CS0834.md) | Look-alike char mix in string literal
| [CS0790](./CS0835.md) | Look-alike char mix in comment
| [CS0905](./CS0905.md) | Argument does not have requested details
| [CS0914](./CS0914.md) | Different literals in INTERSECT/EXCEPT construction
| [CS0917](./CS0917.md) | Forbidden INSERT hint is used
| [CS0919](./CS0919.md) | Parameter is defined but never used
| [CS0920](./CS0920.md) | Missing pair of transaction state command
| [CS0921](./CS0921.md) | Missing pair of XML document state command
| [CS0922](./CS0922.md) | Parameter is expected to be OUTPUT
| [CS0923](./CS0923.md) | Unexpected sorting of insert source
| [CS0924](./CS0924.md) | Input parameter value overwritten without attempt to read it
| [CS0930](./CS0930.md) | Cursor name reused
| [CS0931](./CS0931.md) | Transaction name reused
| [CS0932](./CS0932.md) | Take a look at comment
| [CS0936](./CS0936.md) | Variable referenced before assignment
| [CS0937](./CS0937.md) | Implicit string truncation
| [CS0938](./CS0938.md) | [NOT] EXISTS is preferred over IN predicate
| [CS0940](./CS0940.md) | Loop predicate never changes inside loop
| [CS0942](./CS0942.md) | Query with recursive CTE should have MAXRECURSION limit
| [CS0946](./CS0946.md) | Literal size is too big
| [CS0950](./CS0950.md) | Last statement in a function must be RETURN
| [CS0954](./CS0954.md) | Loop based on @@FETCHSTATUS has no FETCH statement inside
| [CS0955](./CS0955.md) | Infinite loop has no exit
| [CS0959](./CS0959.md) | Duplicate expression in conditional flow has no effect
| [CS0960](./CS0960.md) | Unreliable type check
| [CS0975](./CS0975.md) | Trigger should be in the same schema as target table
| [CS0976](./CS0976.md) | Dropping not owned table from outer scope is unsafe
| [CS0977](./CS0977.md) | Dropping without existence check is not error-prone
| [CS0982](./CS0982.md) | Cursor should be defined as FORWARD_ONLY or FAST_FORWARD
| [CS0984](./CS0984.md) | Variable is self-assigned
| [CS0985](./CS0985.md) | Variable is self-compared
| [CS0990](./CS0990.md) | Keyword is used for transaction name
| [CS0991](./CS0991.md) | WAITFOR DELAY usage is not welcome
| [CS0992](./CS0992.md) | WAITFOR conversation without timeout limit
| [CS0993](./CS0993.md) | Loss of precision is possible when FLOAT type variables is used
| [CS0994](./CS0994.md) | Loss of precision is possible when FLOAT type columns is used
| [CS0995](./CS0995.md) | Mixing accurate type with FLOAT or REAL

[To docs homepage](./readme.md)
