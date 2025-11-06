CREATE TABLE dbo.acme
(
    id int not null,
    rn rowversion not null
)
GO
CREATE index ix_bar
on dbo.acme(id)
include(rn)
