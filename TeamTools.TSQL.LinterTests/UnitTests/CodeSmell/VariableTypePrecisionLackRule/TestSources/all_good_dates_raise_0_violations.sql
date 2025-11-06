            DECLARE
@dt  DATETIME,
@tm  TIME,
@day DATE,
@n   DECIMAL(10,2) -- ignored

            SELECT
DATEDIFF(MS, @dt, GETDATE()),
DATEDIFF(MONTH, @dt, GETDATE()),
DATEDIFF(MS, @tm, GETDATE()),
DATEDIFF(MONTH, @day, GETDATE()),
DATEDIFF(MONTH, @day, dbo.GET_DATE()), -- unknown function
DATEADD(MS, 1, @dt),
DATEADD(MONTH, 1, @dt),
DATEADD(MS, 1, @tm),
DATEADD(MONTH, 1, @day),
YEAR(@day);
