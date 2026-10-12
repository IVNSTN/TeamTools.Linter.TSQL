DECLARE
    @tm  TIME

SELECT
    DATEDIFF(MONTH, @tm, GETDATE()),
    DATEPART(DD, @tm),
    YEAR(@tm);
