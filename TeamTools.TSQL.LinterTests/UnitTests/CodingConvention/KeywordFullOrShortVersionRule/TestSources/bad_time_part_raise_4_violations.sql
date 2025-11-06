DECLARE @dt DATETIME2(7)

SELECT
    DATEPART(ns, @dt),
    DATENAME(s, @dt),
    DATEADD (mi, 0, @dt),
    DATEDIFF(hh, 0, @dt);
