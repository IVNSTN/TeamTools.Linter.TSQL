DECLARE @foo NCHAR(5)
    , @bar CHAR(5)

SET @bar = @foo         -- foo unassigned

SET @foo = N'☀️'
SELECT @foo = NULL

SET @bar = @foo         -- foo is precisely NULL

SET @bar = NULL
