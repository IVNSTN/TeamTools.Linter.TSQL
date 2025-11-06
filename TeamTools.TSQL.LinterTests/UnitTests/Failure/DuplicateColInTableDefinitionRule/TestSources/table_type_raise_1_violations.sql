CREATE TYPE dbo.tbl_type AS TABLE
(
    id      INT          NOT NULL
    , title VARCHAR(100) NULL
    , title VARCHAR(200) NULL -- intentional dup
    , dt    DATETIME     NOT NULL
);
