CREATE PROCEDURE dbo.make_web_request
    @url   NVARCHAR(4000)
    , @err NVARCHAR(MAX) OUTPUT
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [Sql.Web.Rest].StoredProcedures.WebRequestInvoke;
