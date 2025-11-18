CREATE FUNCTION dbo.string_fn (@src VARCHAR(100))
RETURNS VARCHAR(100)
AS
BEGIN
    RETURN GETDATE()    -- 1 this is not VARCHAR
END;
GO

CREATE FUNCTION dbo.date_fn (@input DATE)
RETURNS DATETIME
AS
BEGIN
    RETURN 'asdf'       -- 2 this is not (valid) DATETIME
END;
GO

CREATE FUNCTION dbo.date_fn (@input DATETIME)
RETURNS INT
AS
BEGIN
    RETURN @input + 1   -- 3 this is not INT (datetime + int = datetime)
END;
GO
