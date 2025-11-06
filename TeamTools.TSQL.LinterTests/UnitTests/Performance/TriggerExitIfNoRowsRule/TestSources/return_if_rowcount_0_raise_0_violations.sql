CREATE TRIGGER foo on dbo.bar FOR INSERT 
AS
BEGIN
    IF @@ROWCOUNT = 0 RETURN;

    DECLARE @a INT = 0;
END
GO
CREATE TRIGGER jad on dbo.bar AFTER DELETE
AS
BEGIN
    IF @@ROWCOUNT = 0
    begin
        RETURN;
    end;

    SELECT 1, 2, 3;

    IF 1=0
        RETURN;
END
GO
CREATE TRIGGER zar on dbo.bar FOR INSERT 
AS
    /* test */
    IF @@rowcount = 0 BEGIN RETURN END;

    DECLARE @a INT = 0;
GO
