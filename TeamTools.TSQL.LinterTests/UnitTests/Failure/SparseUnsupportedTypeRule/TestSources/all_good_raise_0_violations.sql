CREATE TABLE dbo.foo
(
    bar INT SPARSE NULL,    -- good for sparse
    geo GEOGRAPHY NOT NULL  -- not sparse
)
GO
