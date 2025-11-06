DECLARE @str CHAR(3)

SET @str = LEFT((SELECT ('adsf')), (5))
SET @str = RIGHT('abcdefgh', 5)
