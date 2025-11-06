DECLARE @foo NCHAR(10)

SELECT FORMAT(@foo, 'c') -- format does not support strings

SET @bar = DATEPART(@foo, @dt) -- bad datepart
