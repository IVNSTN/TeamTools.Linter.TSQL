-- compatibility level min: 130
CREATE procedure dbo.foo
as
begin
    CREATE TABLE dbo.acme
    (
        id int not null,
        rn timestamp not null,
        index ix (rn)
    )
END;
