CREATE TABLE dbo.tbl
(
    id          INT NOT NULL
    , some_calc AS (1 + 1)
    , CONSTRAINT PK_tbl PRIMARY KEY (id)
)
GO

CREATE INDEX idx1 on dbo.tbl(id)
INCLUDE(some_calc)
GO

CREATE INDEX idx2 on dbo.tbl(id, some_calc)
GO
