CREATE PROCEDURE dbo.make_web_request
    @url            NVARCHAR(4000)
    , @errorMessage NVARCHAR(MAX) OUTPUT
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [Sql.Web.Rest].StoredProcedures.WebRequestInvoke;

-- Without GO the next statement is treated as invalid syntax
GO
GRANT EXEC ON dbo.make_web_request TO PUBLIC AS dbo;
