CREATE PROC dbo.test
AS
BEGIN
    CREATE TABLE dbo.acme
    (
        id int not null,
        a_date date not null
    )on Date2fsMessBess(a_date);
    
    CREATE INDEX ix_foo
    on dbo.acme(id)
END;
