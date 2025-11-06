CREATE TRIGGER foo on dbo.bar FOR INSERT 
AS
BEGIN
    DECLARE @a INT = 0;

    exec dbo.test

    IF @@ROWCOUNT = 0
    begin
        RETURN;
    end;
END
GO
CREATE TRIGGER jad on dbo.bar AFTER DELETE
AS
BEGIN
    IF 1=0
        RETURN;

    SET NOCOUNT ON;

    SELECT 1, 2, 3;

    IF @@ROWCOUNT = 0
    begin
        RETURN;
    end;
END
GO
CREATE TRIGGER sad on dbo.bar AFTER DELETE
AS
    SET NOCOUNT ON;

    IF @@ROWCOUNT = 0
    BEGIN
        RETURN;
        END
GO
CREATE TRIGGER sad on dbo.bar AFTER DELETE
AS
    IF @@ROWCOUNT <> 0
        RETURN;
