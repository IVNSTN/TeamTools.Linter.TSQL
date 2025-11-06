CREATE TABLE dbo.acme
(
    id   INT NOT NULL
    , name VARCHAR(10) NULL
    , date DATE
    , CONSTRAINT PK_FOO PRIMARY KEY CLUSTERED (id, name, date)
)
