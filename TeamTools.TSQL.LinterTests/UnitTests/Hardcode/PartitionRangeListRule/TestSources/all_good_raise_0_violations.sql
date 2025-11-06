CREATE INDEX IX_FOO
ON dbo.BAR (t)
WITH (DATA_COMPRESSION = PAGE)
ON f(r);

CREATE TABLE dbo.my_table
(
    id int not null primary key clustered WITH (DATA_COMPRESSION = PAGE)
)
WITH (DATA_COMPRESSION = PAGE)
