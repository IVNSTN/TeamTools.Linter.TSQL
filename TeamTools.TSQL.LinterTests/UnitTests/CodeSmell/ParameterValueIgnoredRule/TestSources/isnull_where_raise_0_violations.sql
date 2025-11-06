CREATE PROC dbo.foo
    @paramA   INT
    , @paramB INT
    , @paramC INT
    , @paramD INT
    , @paramE INT
    , @paramF INT
    , @paramG INT
    , @paramH INT
AS
BEGIN
    SET @paramA = ISNULL(@paramA, 0); -- expression happens before assignment

    SELECT
        @paramB = CASE WHEN @paramB IS NULL THEN 1 ELSE @paramB + 1 END -- CASE happens before SET
        , @paramC = 0
    WHERE @paramC <> 0; -- WHERE happens before SELECT

    SELECT
        @paramD = 0
        , @paramE = 0
        , @paramF = 0
    FROM dbo.foo AS f
    INNER JOIN dbo.bar AS b
        ON b.id = f.id
    INNER JOIN dbo.gar AS g
        ON g.id = b.id + @paramD -- FROM happens before SELECT
    INNER JOIN (VALUES (@paramF)) AS v (id)
        ON v.id = g.id
    OUTER APPLY
    (SELECT @paramE) AS e;

    SELECT
        @paramG = 0
        , @paramH = 0
    FROM dbo.foo AS f
    GROUP BY @paramG + 1 -- GROUP BY and HAVING happen before SELECT
    HAVING @paramH = 0;
END;
