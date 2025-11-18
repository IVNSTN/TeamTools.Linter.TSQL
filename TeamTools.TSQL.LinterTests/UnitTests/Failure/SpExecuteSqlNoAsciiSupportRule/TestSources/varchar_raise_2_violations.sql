declare @script CHAR(100) = 'select 1'
exec sp_executesql @script              -- 1
GO

sp_executesql 'PRINT ''OOPS'''          -- 2
GO
