WHILE 1=0
BEGIN
    PRINT '1'
END
GO

WHILE @@FETCH_STATUS = 0
BEGIN
    PRINT '1'
END
GO

WHILE @a > @b
BEGIN
    PRINT '1'
END
GO

WHILE @sect_num <> 255
BEGIN
    PRINT '1'
END
GO

-- no always true predicate
WHILE @sect_num <> 255 OR 1=0
BEGIN
    PRINT '1'
END
GO

-- one true is not enough
WHILE @sect_num <> 255 AND 1=1
BEGIN
    PRINT '1'
END
GO
