CREATE TABLE dbo.foo
(
    id int not null
    , a VARCHAR(10) NULL
    , CONSTRAINT PK_foo PRIMARY KEY(id)
)
GO
CREATE INDEX idx_a on dbo.foo(a)
include(id)
GO
