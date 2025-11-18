declare @script NCHAR(100) = N'select 1'
exec sp_executesql @script
GO

sp_executesql N'PRINT ''ALRIGHT'''
GO

declare @args NCHAR(100) = N'@msg VARCHAR(100)'

EXEC sp_executesql N'PRINT @msg', @args, @msg = 'OK'
GO
