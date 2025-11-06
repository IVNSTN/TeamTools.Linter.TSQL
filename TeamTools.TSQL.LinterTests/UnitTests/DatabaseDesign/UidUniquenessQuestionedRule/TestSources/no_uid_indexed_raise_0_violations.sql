CREATE TABLE dbo.foo
(
    row_id  INT NOT NULL
    , dt    DATETIME
    , title VARCHAR(100)
    , CONSTRAINT PK PRIMARY KEY (dt)
)

CREATE NONCLUSTERED INDEX IX ON dbo.foo (row_id, title)

CREATE UNIQUE NONCLUSTERED INDEX IX ON dbo.foo (title)
