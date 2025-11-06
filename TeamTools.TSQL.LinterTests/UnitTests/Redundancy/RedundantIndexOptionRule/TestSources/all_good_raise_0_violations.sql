CREATE TABLE dbo.foo
(
    id INT,
    CONSTRAINT PK PRIMARY KEY CLUSTERED (id) -- no options
)
GO

DECLARE @tbl TABLE
(
    id INT -- no key
)
GO

CREATE TABLE dbo.foo
(
    id INT,
    CONSTRAINT PK PRIMARY KEY CLUSTERED (id) -- options have non default value
    WITH (PAD_INDEX = ON, ALLOW_ROW_LOCKS = OFF)
)
GO
