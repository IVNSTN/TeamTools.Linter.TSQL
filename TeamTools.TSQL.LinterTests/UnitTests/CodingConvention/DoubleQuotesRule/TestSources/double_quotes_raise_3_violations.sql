CREATE TABLE tmp.[foo bar](id INT)
INSERT tmp.[foo bar] VALUES (1), (2)
GO

SET QUOTED_IDENTIFIER OFF

SELECT
    "asdf" AS [my col] -- 1
FROM tmp.[foo bar]
GO

SET QUOTED_IDENTIFIER ON

SELECT
    'asdf' AS "my col" -- 2
FROM tmp."foo bar" -- 3
GO
