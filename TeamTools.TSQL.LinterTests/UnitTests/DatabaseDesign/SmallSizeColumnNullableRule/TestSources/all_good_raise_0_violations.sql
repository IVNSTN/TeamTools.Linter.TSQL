-- long types
CREATE TABLE dbo.foo
(
    title  VARCHAR(300) NULL,
    flag   INT NULL,
    calc   AS flag * 2 -- computed col
)
GO

-- sparse cols
CREATE TABLE dbo.foo
(
    title  VARCHAR(300) SPARSE NULL ,
    flag   INT SPARSE NULL
)
GO

-- not null
CREATE TABLE dbo.foo
(
    flag   BIT      NOT NULL,
    prefix CHAR(2)  NOT NULL
)
