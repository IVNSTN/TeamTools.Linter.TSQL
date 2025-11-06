CREATE TYPE dbo.my_tbl_type AS TABLE
(
    id          INT NOT NULL IDENTITY(1,1)
    , txt       VARCHAR(100) NOT NULL DEFAULT RIGHT('dummy', 1) -- 1 - RIGHT
    , CHECK (RIGHT(txt, 1) <> '') -- 2
)
