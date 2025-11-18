SELECT *
FROM Sales.Customer TABLESAMPLE SYSTEM(10 PERCENT);


SELECT d.*, p.Score
FROM PREDICT(MODEL = @model,
    DATA = dbo.mytable AS d) WITH (Score FLOAT) AS p;
