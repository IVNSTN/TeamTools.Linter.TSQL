DECLARE @mytbl TABLE
(
    id INT,
    dt DATE
)

INSERT @mytbl(id, dt)
VALUES
(NEWID(), GETDATE())
, (0, CAST(SYSDATETIME() AS TIME))
GO

CREATE TABLE #mytbl
(
    id INT
    , dt DATE
)

INSERT #mytbl(dt, id) -- reverse order
SELECT
    GETDATE()
    , NEWID()


INSERT #mytbl(dt, id) -- reverse order
SELECT
    CAST('' AS TIME)
    , 0
GO
