CREATE TRIGGER dbo.trg ON dbo.foo
AFTER INSERT, UPDATE
AS
BEGIN
    INSERT dbo.foo(name)
    values ('asdf')
END
GO

CREATE TRIGGER dbo.trg1 ON dbo.foo
AFTER INSERT, UPDATE
AS
BEGIN
    update t set
        dtupdate = GETDATE()
    FROM dbo.foo as t;
END
GO

CREATE TRIGGER dbo.trg2 ON dbo.foo
AFTER DELETE
AS
BEGIN
    DELETE f FROM dbo.foo f
    INNER JOIN DELETED d
        on d.id= f.id

    merge dbo.foo f
    using deleted d
    on f.id = d.id
    when matched then delete;
END
