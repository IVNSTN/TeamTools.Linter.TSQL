CREATE TABLE dbo.acme
(
    id int not null,
    rN TIMESTAMP not null
)
GO
CREATE index ix_bar
on dbo.acme(rn)
