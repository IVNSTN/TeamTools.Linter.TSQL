declare @foo VARCHAR(100),
    @bar VARBINARY(1),
    @zar numeric(17,2),
    @dt DATETIME2(7)

SET @foo = CAST(@bar as VARCHAR(10));
SET @foo = CONVERT(VARCHAR(10), GETDATE(), 121);
