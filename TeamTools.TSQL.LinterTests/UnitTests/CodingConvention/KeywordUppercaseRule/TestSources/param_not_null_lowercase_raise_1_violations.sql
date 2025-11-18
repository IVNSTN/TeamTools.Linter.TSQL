-- compatibility level min: 130
CREATE PROCEDURE dbo.foo
    @arg INT not NULL       -- here
WITH EXECUTE AS CALLER
AS
EXTERNAL NAME [Sql.Web.Rest].StoredProcedures.WebRequestInvoke;
GO
