-- compatibility level min: 110
;THROW 50001,   'test',1; -- missing space and extra space
SET @a = IIF(1=2,@b,   @c); -- missing space and extra space
