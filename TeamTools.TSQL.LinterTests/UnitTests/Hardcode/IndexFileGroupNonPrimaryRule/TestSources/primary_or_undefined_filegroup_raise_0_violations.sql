-- all to primary
CREATE TABLE dbo.foo
(
    id INT
    , name VARCHAR(100)
    , CONSTRAINT PK_foo PRIMARY KEY CLUSTERED (id) ON [PRIMARY]
) ON [PRIMARY]

CREATE INDEX IX_foo on dbo.foo(name)
ON [PRIMARY]
GO

-- no fg at all
CREATE TABLE dbo.bar
(
    id INT
    , name VARCHAR(100)
    , sect_num as id % 10
    , CONSTRAINT PK_foo PRIMARY KEY CLUSTERED (id)
) on prtScheme(sect_num)

CREATE INDEX IX_foo on dbo.foo(name)
