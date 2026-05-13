-- compatibility level min: 130
INSERT INTO @codes (code)
SELECT DISTINCT
    TRY_CAST(value AS VARCHAR(20)) AS code
FROM STRING_SPLIT(@code_list, ',')
WHERE value <> '';
