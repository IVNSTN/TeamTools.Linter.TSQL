DECLARE @foo NCHAR(5) = N'☀️'
    , @bar NVARCHAR(MAX)

SET @bar = CONCAT(N'☁️', ' ') + @foo

PRINT @bar
