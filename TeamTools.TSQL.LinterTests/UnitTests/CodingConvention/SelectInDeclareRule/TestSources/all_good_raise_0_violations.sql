DECLARE @foo INT = 0 * 1,
    @bar VARCHAR(10) = 'SELECT',
    @zar BIT,
    @nar DATETIME = GETDATE(),
    @trn INT = @@TRANCOUNT;

    SELECT @foo = @bar
