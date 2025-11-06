CREATE TABLE dbo.foo
(
    id int not null identity(1,1),
    group_id int null,
    name varchar(20) default '',
    CONSTRAINT FK_GROUP FOREIGN KEY (group_id) REFERENCES dbo.foo_group(id),
    CONSTRAINT PK_FOO PRIMARY KEY (id),
    CONSTRAINT CK_name CHECK (group_id IS NOT NULL OR name = 'NULL')
)

CREATE TABLE #foo
(
    id int not null identity(1,1) PRIMARY KEY,
    group_id int null FOREIGN KEY (group_id) REFERENCES dbo.foo_group(id),
    name varchar(20) default '' CHECK (group_id IS NOT NULL OR name = 'NULL')
)
