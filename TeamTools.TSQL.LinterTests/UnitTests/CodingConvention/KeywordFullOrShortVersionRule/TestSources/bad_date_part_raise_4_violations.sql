DECLARE @dt DATETIME2(7)

SELECT
    DATEPART(YY, @dt),
    DATENAME(M, @dt),
    DATEADD (DD, 0, @dt),
    DATEDIFF(Q, 0, @dt);
