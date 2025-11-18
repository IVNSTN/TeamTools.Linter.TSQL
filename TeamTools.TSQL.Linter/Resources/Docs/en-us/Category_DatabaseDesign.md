
# Rules of DatabaseDesign category

|||
|-|-|
| [DD0153](./DD0153.md) | Composite foreign key
| [DD0158](./DD0158.md) | All table columns can contain a NULL value
| [DD0182](./DD0182.md) | The ROWVERSION column must be NOT NULL
| [DD0183](./DD0183.md) | The IDENTITY column must be NOT NULL
| [DD0184](./DD0184.md) | The primary key column must be NOT NULL
| [DD0701](./DD0701.md) | The unique column is included in unique constraint
| [DD0749](./DD0749.md) | Variable-length column with small size
| [DD0750](./DD0750.md) | Fixed-length column with big size
| [DD0755](./DD0755.md) | Small size column must be NOT NULL
| [DD0758](./DD0758.md) | Redundant use of the LOCK_ESCALATION option
| [DD0759](./DD0759.md) | For partitioned tables, it is recommended to use the LOCK_ESCALATION = AUTO option
| [DD0765](./DD0765.md) | Trigger on in-memory table
| [DD0772](./DD0772.md) | SPARSE column of small size
| [DD0773](./DD0773.md) | SPARSE column of MAX size
| [DD0774](./DD0774.md) | SPARSE column for in-memory
| [DD0777](./DD0777.md) | SPARSE columns should not be used
| [DD0778](./DD0778.md) | SPARSE columns don't dominate the table
| [DD0799](./DD0799.md) | Default 'NULL' defined as text
| [DD0824](./DD0824.md) | Short text field allows unicode
| [DD0825](./DD0825.md) | Single-column table
| [DD0826](./DD0826.md) | Table has too many columns
| [DD0827](./DD0827.md) | Computed column result is constant
| [DD0828](./DD0828.md) | Computed column mirrors other column
| [DD0829](./DD0829.md) | Foreign keys with temporary tables
| [DD0906](./DD0906.md) | Recursive foreign key
| [DD0908](./DD0908.md) | Nonclustered index includes clustered index columns
| [DD0909](./DD0909.md) | Column is included in the index more than once
| [DD0913](./DD0913.md) | NOCHECK CONSTRAINT is used
| [DD0941](./DD0941.md) | Table is missing a primary key
| [DD0997](./DD0997.md) | Type is not recommended for keys
| [DD0998](./DD0998.md) | Type is not recommended for clustered index
| [DD0999](./DD0999.md) | Bloated clustered index

[To docs homepage](./readme.md)
