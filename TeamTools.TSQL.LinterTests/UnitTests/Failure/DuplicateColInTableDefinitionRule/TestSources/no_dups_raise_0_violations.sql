DECLARE @foo TABLE
(
    id         INT          NOT NULL
    , title    VARCHAR(100) NULL
    , subtitle VARCHAR(200) NULL -- no dup
    , dt       DATETIME     NOT NULL
);
GO

CREATE TABLE dbo.foo
(
    id         INT          NOT NULL
    , title    VARCHAR(100) NULL
    , subtitle VARCHAR(200) NULL -- no dup
    , dt       DATETIME     NOT NULL
);
GO

CREATE TABLE #foo
(
    id         INT          NOT NULL
    , title    VARCHAR(100) NULL
    , subtitle VARCHAR(200) NULL -- no dup
    , dt       DATETIME     NOT NULL
);
GO

CREATE TYPE dbo.tbl_type AS TABLE
(
    id         INT          NOT NULL
    , title    VARCHAR(100) NULL
    , subtitle VARCHAR(200) NULL -- no dup
    , dt       DATETIME     NOT NULL
);
GO

CREATE FUNCTION dbo.foo ()
RETURNS @result TABLE
(
    id         INT          NOT NULL
    , title    VARCHAR(100) NULL
    , subtitle VARCHAR(200) NULL -- no dup
    , dt       DATETIME     NOT NULL
)
AS
BEGIN
    SELECT * FROM dbo.barа;

    RETURN;
END;
GO

CREATE FUNCTION dbo.my_split_fn (@str VARCHAR(4000), @delim CHAR(1))
RETURNS TABLE (substr VARCHAR(100) NOT NULL)
AS
    EXTERNAL NAME StringAggregates.foo.bar;
GO
