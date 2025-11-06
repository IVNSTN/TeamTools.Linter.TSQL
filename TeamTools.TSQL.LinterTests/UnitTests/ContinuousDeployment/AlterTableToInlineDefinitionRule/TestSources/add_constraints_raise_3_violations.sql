CREATE TABLE dbo.foo
(
    id            INT
    , title       VARCHAR(100)
    , foo_type_id INT
);
GO

ALTER TABLE dbo.foo
ADD CONSTRAINT DF_title DEFAULT '' FOR title; -- 1
GO

ALTER TABLE dbo.foo
ADD CONSTRAINT PK PRIMARY KEY (id); -- 2
GO

ALTER TABLE dbo.foo
ADD CONSTRAINT CK CHECK (foo_type_id > 0); -- 3
GO
