CREATE TRIGGER dbo.trg ON dbo.foo
AFTER INSERT, UPDATE
AS
BEGIN
    INSERT dbo.bar(name)
    values ('asdf')

    update #tmp
    set dtupdate = getdate();

    -- not a modification
    select * from dbo.foo

    -- not a circular action
    delete dbo.foo where 1=0;
END
GO

CREATE TRIGGER dbo.trg2 ON dbo.foo
AFTER DELETE
AS
BEGIN
    delete dbo.bar
    where 1=1
END
GO

CREATE TRIGGER dbo.trg ON dbo.foo
INSTEAD OF INSERT, UPDATE
AS
BEGIN
    INSERT dbo.bar(name)
    values ('asdf')

    update #tmp
    set dtupdate = getdate();
END
GO

CREATE TRIGGER dbo.trg2 ON dbo.foo
INSTEAD OF DELETE
AS
BEGIN
    delete dbo.bar
    where 1=1
END
