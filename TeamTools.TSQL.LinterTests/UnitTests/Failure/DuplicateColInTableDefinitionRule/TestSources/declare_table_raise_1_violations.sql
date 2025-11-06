DECLARE @foo TABLE
(
    id      INT          NOT NULL
    , title VARCHAR(100) NULL
    , title VARCHAR(200) NULL -- intentional dup
    , dt    DATETIME     NOT NULL
);
GO
