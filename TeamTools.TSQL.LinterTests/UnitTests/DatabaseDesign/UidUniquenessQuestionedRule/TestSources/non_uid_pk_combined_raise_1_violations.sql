CREATE TABLE dbo.foo
(
    row_id  INT NOT NULL
    , dt    DATETIME
    , title VARCHAR(100)
    , CONSTRAINT PK PRIMARY KEY (dt) -- dt is pk
)

CREATE UNIQUE NONCLUSTERED INDEX IX ON dbo.foo (dt, row_id) -- 1
