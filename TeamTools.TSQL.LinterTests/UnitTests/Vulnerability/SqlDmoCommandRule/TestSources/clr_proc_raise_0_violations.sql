CREATE PROCEDURE dbo.foo
    @x   INT
    , @y INT
AS
EXTERNAL NAME assm.bar.method;
GO
