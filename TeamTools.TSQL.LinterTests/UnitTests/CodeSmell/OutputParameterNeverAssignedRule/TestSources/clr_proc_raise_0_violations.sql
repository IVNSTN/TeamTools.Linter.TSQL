CREATE PROCEDURE dbo.make_web_request
    @url            NVARCHAR(4000)
    , @xmlRequest   NVARCHAR(MAX)
    , @method       NVARCHAR(100)
    , @resposne     NVARCHAR(MAX) OUTPUT
    , @errorMessage NVARCHAR(MAX) OUTPUT
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [Sql.Web.Rest].StoredProcedures.WebRequestInvoke;
GO
