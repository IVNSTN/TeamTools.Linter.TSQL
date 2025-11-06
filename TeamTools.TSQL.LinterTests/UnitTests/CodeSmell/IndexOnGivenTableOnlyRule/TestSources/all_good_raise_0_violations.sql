-- compatibility level min: 130
CREATE TABLE dbo.test
(
    id int not null,
    index ix_dbo_test_id2 nonclustered (id));
GO
CREATE INDEX ix_test
on dbo.test(id)
on [PRIMARY]
