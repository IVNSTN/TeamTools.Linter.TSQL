            DECLARE
@tm  TIME,
@day DATE

            SELECT
DATEDIFF(MS, @day, GETDATE()),
DATEDIFF(MONTH, @tm, GETDATE()),
DATEADD(MS, 1, @day),
DATEADD(MONTH, 1, @tm),
YEAR(@tm);
