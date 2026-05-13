CREATE FUNCTION dbo.foo (@Certificate VARBINARY(8000), @SignText VARBINARY(8000))
RETURNS NVARCHAR(4000)
AS
    EXTERNAL NAME CryptoSQLLibs.CryptoSQLLibs.VerifyCryptoMessage;
GO

CREATE PROCEDURE dbo.bar (@Certificate VARBINARY(8000), @SignText VARBINARY(8000))
AS
    EXTERNAL NAME CryptoSQLLibs.CryptoSQLLibs.VerifyCryptoMessage;
GO

CREATE TRIGGER dbo.far ON dbo.jar AFTER INSERT, UPDATE, DELETE
AS
    EXTERNAL NAME CryptoSQLLibs.CryptoSQLLibs.VerifyCryptoMessage;
GO
