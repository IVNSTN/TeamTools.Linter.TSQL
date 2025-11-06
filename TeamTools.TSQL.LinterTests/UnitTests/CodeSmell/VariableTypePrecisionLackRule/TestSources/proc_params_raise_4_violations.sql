            CREATE PROCEDURE dbo.my_proc
@tm  TIME,
@day DATE
            AS
            BEGIN
DECLARE @dt DATETIME2(7)
SET @dt = DATEDIFF(NS, @day, GETDATE());

SELECT
    DATEDIFF(DAY, @tm, GETDATE()),
    DATEADD(SS, 1, @day),
    DATEADD(YEAR, 1, @tm);
            END;
