-- compatibility level min: 110
IF ABS(@e - 3 / 200) * @d + 3.15 > IIF(@a = 1, DATEDIFF(DD, GETDATE(), @dt), 0)
OR @e + d <= @c
    PRINT 1
ELSE IF @c = @d
    PRINT 2
ELSE IF ABS(@e - 3 / 200) * @d + 3.15 > IIF(@a = 1, DATEDIFF(DD, GETDATE(), @dt), 0) OR @e + d <= @c -- here
    PRINT 3
GO
