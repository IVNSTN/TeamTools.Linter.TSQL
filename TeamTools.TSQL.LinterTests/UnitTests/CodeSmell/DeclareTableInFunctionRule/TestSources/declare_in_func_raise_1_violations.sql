CREATE FUNCTION foo.bar (@some_id INT)
RETURNS @res TABLE
(
    value_a         NUMERIC(14, 2) NOT NULL
    , value_b       NUMERIC(14, 2) NOT NULL
)
AS
BEGIN
    DECLARE @calc TABLE     -- here
    (
        asdf INT
        , qwer INT
    )
END
