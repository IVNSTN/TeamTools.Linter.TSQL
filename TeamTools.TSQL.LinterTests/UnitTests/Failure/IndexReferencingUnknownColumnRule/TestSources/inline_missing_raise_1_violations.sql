-- compatibility level min: 130
CREATE TABLE dbo.tbl
(
    id          INT NOT NULL
    , parent_id INT NULL
    , name      VARCHAR(10) NULL
    , INDEX ix UNIQUE (asdf)              -- here
)
GO
