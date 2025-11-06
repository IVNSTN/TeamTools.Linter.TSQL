CREATE TRIGGER dbo.foo_trigger
ON dbo.foo
FOR INSERT
NOT FOR REPLICATION
AS
/*
some

miltiline

comment


*/
BEGIN
    IF @@ROWCOUNT = 0
    BEGIN
        RETURN;
    END;
END;
