
GO

DECLARE
    @foo   INT
    , @bar VARCHAR(10);
GO

DECLARE
    @foo   INT         = (1 - 2 * 3)
    , @bar VARCHAR(10) = ''
    , @dt  DATETIME    = DATEADD(DD, -1, GETDATE());
GO
