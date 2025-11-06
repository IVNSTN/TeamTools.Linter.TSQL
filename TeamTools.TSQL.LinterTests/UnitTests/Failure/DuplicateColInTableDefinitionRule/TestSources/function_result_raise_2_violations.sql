CREATE FUNCTION dbo.foo ()
RETURNS @result TABLE
(
    id      INT          NOT NULL
    , title VARCHAR(100) NULL
    , title VARCHAR(200) NULL -- intentional dup
    , dt    DATETIME     NOT NULL
)
AS
BEGIN
    SELECT * FROM dbo.barа;

    RETURN;
END;
GO

CREATE FUNCTION dbo.my_split_fn (@str VARCHAR(4000), @delim CHAR(1))
RETURNS TABLE
(
    id      INT          NOT NULL
    , title VARCHAR(100) NULL
    , title VARCHAR(200) NULL -- intentional dup
    , dt    DATETIME     NOT NULL
)
AS
    EXTERNAL NAME StringAggregates.foo.bar;
GO
