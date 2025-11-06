SELECT FORMAT(@var, 'C', 'en-us', 2, 3) -- 1

SET @bar = DATEPART(DAY, @dt, 1, MONTH, 3) -- 2
