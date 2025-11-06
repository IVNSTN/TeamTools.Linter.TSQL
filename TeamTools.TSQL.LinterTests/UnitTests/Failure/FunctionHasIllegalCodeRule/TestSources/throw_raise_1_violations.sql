-- compatibility level min: 110
CREATE FUNCTION my_fn()
RETURNS INT
BEGIN;
    THROW 50000, 'error', 1;
    RETURN 1;
END
GO
