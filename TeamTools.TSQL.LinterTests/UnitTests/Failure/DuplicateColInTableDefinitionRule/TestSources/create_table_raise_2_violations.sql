CREATE TABLE dbo.foo
(
    id      INT          NOT NULL
    , title VARCHAR(100) NULL
    , title VARCHAR(200) NULL -- intentional dup
    , dt    DATETIME     NOT NULL
);
GO

CREATE TABLE #foo
(
    id      INT          NOT NULL
    , title VARCHAR(100) NULL
    , title VARCHAR(200) NULL -- intentional dup
    , dt    DATETIME     NOT NULL
);
