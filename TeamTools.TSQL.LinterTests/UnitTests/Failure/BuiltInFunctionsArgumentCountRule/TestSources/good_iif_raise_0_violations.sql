-- compatibility level min: 110
DECLARE @str VARCHAR(10);

SELECT
    IIF(NULL = 1,2, 3) as x
