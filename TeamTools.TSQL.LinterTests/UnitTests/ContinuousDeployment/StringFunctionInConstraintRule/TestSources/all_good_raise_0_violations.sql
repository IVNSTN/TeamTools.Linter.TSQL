CREATE TABLE dbo.my_tbl
(
    id        INT NOT NULL IDENTITY(1,1)
    , txt     VARCHAR(100) NOT NULL DEFAULT SUBSTRING('dummy', 1, 2)
    , usrname VARCHAR(100) NOT NULL DEFAULT (ORIGINAL_LOGIN())
    , dummy   AS SUBSTRING('dummy', 1, 2)
)

DECLARE @my_tbl TABLE
(
    id        INT NOT NULL IDENTITY(1,1)
    , txt     VARCHAR(100) NULL DEFAULT LEFT('dummy', 1) -- must be ignored
    , usrname VARCHAR(100) NOT NULL DEFAULT (ORIGINAL_LOGIN())
)

CREATE TABLE #my_tbl
(
    id        INT NOT NULL IDENTITY(1,1)
    , txt     VARCHAR(100) NOT NULL DEFAULT RIGHT('dummy', 1) -- must be ignored
    , usrname VARCHAR(100) NOT NULL DEFAULT (ORIGINAL_LOGIN())
)

ALTER TABLE dbo.my_tbl
ADD CONSTRAINT cstr CHECK (SUBSTRING(txt,1,1) = 'adsf')

-- must be ignored
ALTER TABLE #my_tbl
ADD CONSTRAINT cstr CHECK (LEFT(txt,1) = 'adsf')
