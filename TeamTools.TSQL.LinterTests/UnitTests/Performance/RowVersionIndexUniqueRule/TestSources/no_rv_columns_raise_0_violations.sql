CREATE TABLE dbo.acme
(
    id int not null,
    rn varchar(10) not null
)
GO
CREATE index ix_bar
on dbo.acme(rn, id)
