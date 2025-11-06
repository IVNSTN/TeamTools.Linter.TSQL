CREATE TABLE dbo.my_tbl
(
    id          INT NOT NULL IDENTITY(1,1)
    , txt       VARCHAR(100) NOT NULL DEFAULT RIGHT('dummy', 1) -- 1 - RIGHT
)

ALTER TABLE dbo.my_tbl
ADD CONSTRAINT cstr CHECK (LEFT(txt,1) = 'adsf') -- 2 - LEFT
