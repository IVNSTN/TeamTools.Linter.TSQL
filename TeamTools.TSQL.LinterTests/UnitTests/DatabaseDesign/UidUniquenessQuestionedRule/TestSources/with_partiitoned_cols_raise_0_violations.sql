CREATE TABLE dbo.foo
(
    row_id  INT NOT NULL IDENTITY(1,1)
    , dt    DATETIME
    , title VARCHAR(100)
    , CONSTRAINT PK PRIMARY KEY (row_id, dt) ON prt(dt)
)
GO
