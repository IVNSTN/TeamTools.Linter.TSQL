CREATE TRIGGER foo on dbo.bar FOR INSERT 
AS
BEGIN
    DECLARE @a INT = 0;

    exec dbo.test
END
GO
CREATE TRIGGER sad on dbo.bar AFTER DELETE
AS
BEGIN
    IF @@ROWCOUNT = 0
    BEGIN
        INSERT a values ('b')
        END

    DECLARE @c int;

    RETURN;
END
GO
CREATE TRIGGER sad on dbo.bar AFTER DELETE
AS
    IF @@ROWCOUNT = 0
    BEGIN
        exec dbo.test;
    END
GO
CREATE TRIGGER zbd on dbo.bar AFTER DELETE
AS
    IF @@ROWCOUNT = 0
        print 'return'
GO
