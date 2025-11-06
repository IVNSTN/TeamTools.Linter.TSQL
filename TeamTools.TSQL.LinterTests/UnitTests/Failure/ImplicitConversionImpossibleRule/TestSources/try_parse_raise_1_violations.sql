-- compatibility level min: 110
DECLARE @x XML -- cannot be implicitly converted into VARCHAR

SELECT TRY_PARSE(@x AS INT)
