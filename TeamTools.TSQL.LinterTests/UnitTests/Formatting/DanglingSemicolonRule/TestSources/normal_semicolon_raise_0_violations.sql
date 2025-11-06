-- compatibility level min: 110
BEGIN;
    ;THROW 500001, 'test', 1; -- leading semicolon is handled by separate rule
END;
