-- compatibility level min: 110
CREATE TABLE dbo.foo
(
    id    INT PRIMARY KEY,
    title VARCHAR(30),
    dt    DATETIME,
)

CREATE CLUSTERED COLUMNSTORE INDEX IX1 ON dbo.foo
