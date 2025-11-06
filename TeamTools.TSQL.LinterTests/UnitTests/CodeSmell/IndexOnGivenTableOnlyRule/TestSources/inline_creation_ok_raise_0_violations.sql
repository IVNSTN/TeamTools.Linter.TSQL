CREATE PROCEDURE dbo.foo
AS
    IF 1=0
        CREATE TABLE tmp.cache (id int null);

    create index is_arch on dbo.archive(a_date);

    drop index is_arch ON tmp.cache;
GO
