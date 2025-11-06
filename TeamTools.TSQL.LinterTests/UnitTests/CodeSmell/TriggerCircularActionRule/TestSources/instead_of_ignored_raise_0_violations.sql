CREATE TRIGGER dbo.trg ON dbo.foo
INSTEAD OF INSERT, UPDATE
AS
BEGIN
    INSERT dbo.foo(name)
    values ('asdf')

    update f set
    dtupdate = GETDATE()
    from dbo.foo f
    where a = b;
END
GO

CREATE TRIGGER dbo.trg2 ON dbo.foo
INSTEAD OF DELETE
AS
BEGIN
    DELETE f FROM dbo.foo f
    INNER JOIN DELETED d
        on d.id= f.id
END
