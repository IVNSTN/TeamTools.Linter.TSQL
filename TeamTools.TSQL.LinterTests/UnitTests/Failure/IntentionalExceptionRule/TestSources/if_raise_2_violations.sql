-- FIXME : it should raise only 1 violation
IF 1 / 0 = 0
BEGIN
    PRINT 'foo'
END
