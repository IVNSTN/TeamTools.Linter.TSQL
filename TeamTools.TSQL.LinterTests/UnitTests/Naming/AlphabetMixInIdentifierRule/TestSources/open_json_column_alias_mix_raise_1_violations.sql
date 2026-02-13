-- compatibility level min: 130
SELECT *
FROM OPENJSON(@data)
WITH (ыi INT);              -- here
