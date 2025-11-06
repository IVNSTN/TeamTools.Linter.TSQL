-- compatibility level min: 150
CREATE TABLE dbo.tbl
(
    id          INT NOT NULL
    , parent_id INT NULL
    , name      VARCHAR(10) NULL
    , INDEX x (parent_id)
    INCLUDE (ancestor_id)                 -- missing
)
GO
