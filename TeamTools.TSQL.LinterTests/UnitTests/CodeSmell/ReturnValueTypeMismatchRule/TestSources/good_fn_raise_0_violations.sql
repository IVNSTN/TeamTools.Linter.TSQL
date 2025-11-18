CREATE FUNCTION dbo.string_fn (@src VARCHAR(100))
RETURNS VARCHAR(100)
AS
BEGIN
    IF @src = 'ASD'
        RETURN NULL;
    ELSE IF @src IS NULL
        RETURN 'asdf'
    ELSE if @src <> 'xzscv'
        RETURN @src
    ELSE
        RETURN TRIM('adsf' + ISNULL(@src, 'XXX'));
END;
GO

CREATE FUNCTION dbo.date_fn (@input DATE)
RETURNS DATETIME
AS
BEGIN
    IF 1=0
        RETURN GETDATE()
    ELSE IF 2=3
        RETURN CAST(@whatever AS DATETIME)
    ELSE IF 3=4
        RETURN @input
    ELSE
        RETURN NULL
END;
GO

CREATE FUNCTION dbo.mny_fn (@input float)
RETURNS SMALLMONEY
AS
BEGIN
    RETURN 0
END;
GO
