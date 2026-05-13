CREATE PROC foo AS;
GO

EXEC ('cmd')
GO

EXEC @var
    @arg1 = @val1
GO


EXEC schm.test
    @arg1 = @val1
GO

EXEC sys.test
    @arg1 = @val1
