-- compatibility level min: 110
WHILE 1=1
BEGIN
    SELECT 1;
    THROW 50000, 'asdf', 1;
    SELECT 2;
END
