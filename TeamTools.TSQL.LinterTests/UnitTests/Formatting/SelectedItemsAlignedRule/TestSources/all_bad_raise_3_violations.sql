SELECT TOP (10) a
, bar.b
FROM bar
GO

SELECT TOP (10)
  bar.c, (b / c) as d
FROM bar

GO

SELECT TOP (10)
      bar.b,
  bar.c
FROM bar
