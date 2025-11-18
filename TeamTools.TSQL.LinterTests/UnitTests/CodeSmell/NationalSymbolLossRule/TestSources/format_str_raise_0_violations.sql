-- Both expressions treat first arg as NVARCHAR
DECLARE @unicode NCHAR(5) = N'☀️-☀️'

-- even with explicit cast
PRINT FORMATMESSAGE(CAST('unicode: %s' AS VARCHAR(100)), @unicode)

RAISERROR ('inicode: %s', 16, 1, N'☀️-☀️-☀️-☀️')
