CREATE TABLE dbo.foo
(
    bar    TINYINT          SPARSE NULL DEFAULT 0      -- 1 default
    , id   INT              SPARSE NULL IDENTITY(1, 1) -- 2 identity
    , far  CHAR(1)          SPARSE NOT NULL            -- 3 not null
    , u_id UNIQUEIDENTIFIER SPARSE NULL ROWGUIDCOL     -- 4 rowguidcol
    , fl   VARBINARY(MAX)   SPARSE FILESTREAM NULL     -- 5 filestream
);
