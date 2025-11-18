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

CREATE FUNCTION dbo.my_trim_fn(@str VARCHAR(4000), @chr CHAR(1))
RETURNS VARCHAR(4000)
AS
    EXTERNAL NAME StringFunctions.foo.bar;
GO
