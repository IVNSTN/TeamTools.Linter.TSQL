CREATE TABLE dbo.foo
(
    row_id  INT NOT NULL IDENTITY(1,1)
    , dt    DATETIME
    , title VARCHAR(100)
    , CONSTRAINT PK PRIMARY KEY (row_id, dt) -- 1
)

CREATE UNIQUE NONCLUSTERED INDEX IX ON dbo.foo (dt, row_id) -- 2
