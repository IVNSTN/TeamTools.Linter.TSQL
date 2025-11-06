-- compatibility level min: 110
BEGIN;
    /* comment */

    THROW 50000, 'asdf', 1
END
GO

SET @a = 1;

-- this is not garbage before semicolon
  -- 
;WITH cte AS (SELECT 1 AS id)
SELECT * FROM cte
GO
