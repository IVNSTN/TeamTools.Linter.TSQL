-- compatibility level min: 150
DECLARE @str VARCHAR(10);

SELECT
    TRIM(' ' FROM 'asdf')
    , ROW_NUMBER() OVER ( ORDER BY (SELECT @str)) as rn;
