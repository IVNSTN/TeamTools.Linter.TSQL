SET NOCOUNT ON;

SELECT 1;

IF @@ROWCOUNT > 0
    RETURN;
GO
SET NOCOUNT ON;

delete dbo.foo

IF @@ROWCOUNT > 0
    RETURN;
GO
SET NOCOUNT OFF;

update dbo.foo set a= b;

IF @@ROWCOUNT > 0
    RETURN;
GO
SET NOCOUNT ON;

exec dbo.my_sp;

IF @@ROWCOUNT > 0
    RETURN;
