-- compatibility level min: 110
SET @f = IIF(@g + NULL = 0, 0, NULL)
