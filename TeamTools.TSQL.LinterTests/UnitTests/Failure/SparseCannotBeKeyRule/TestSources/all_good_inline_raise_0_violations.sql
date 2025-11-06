-- compatibility level min: 130
-- not sparse
CREATE TABLE dbo.foo
(
    group_id INT NOT NULL PRIMARY KEY
    , INDEX ix CLUSTERED (group_id)
) ON prt(group_id)
GO

CREATE TABLE dbo.bar
(
    group_id INT NOT NULL
    , col    BIT SPARSE  NULL
    , INDEX ix NONCLUSTERED (col) -- nonclustered is fine
)
GO
