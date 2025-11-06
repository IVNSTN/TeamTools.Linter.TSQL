CREATE TABLE dbo.tbl
(
    id          INT NOT NULL
    , parent_id INT NULL
    , name      VARCHAR(10) NULL
    , CONSTRAINT PK_tbl PRIMARY KEY (idd)   -- 1
)
GO
CREATE INDEX idx_parent_id
on dbo.tbl(shmarent_id)                     -- 2
GO
CREATE INDEX idx_parent_2
on dbo.tbl(parent_id)
INCLUDE(last_name)                          -- 3
