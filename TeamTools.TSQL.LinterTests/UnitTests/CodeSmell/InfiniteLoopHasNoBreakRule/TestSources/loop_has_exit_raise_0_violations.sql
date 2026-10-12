WHILE 1=1
BEGIN
    IF 0=1
        BREAK
END
GO

WHILE 0=0
BEGIN
    RETURN
END
GO

-- at least one is true
WHILE 0=((0)) OR 1=0
BEGIN
    RETURN
END
GO

-- at least one is true
WHILE 0=0 AND 1=(1)
BEGIN
    RETURN
END
GO
