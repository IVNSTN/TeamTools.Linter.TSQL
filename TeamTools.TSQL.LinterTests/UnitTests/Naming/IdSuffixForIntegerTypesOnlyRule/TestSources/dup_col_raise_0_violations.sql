CREATE TABLE dbo.bar
(
    group_id   INT NOT NULL
    , group_id INT NOT NULL  -- this is an error but it should not ruin parsing
)
GO
