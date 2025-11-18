DECLARE @s VARCHAR(100) = dbo.unknown_fn()

IF REPLACE(@s, 'X', 'Y') > 'XXX asdf'       -- greater is ignored
OR ('A' + 'B') = 'AB'                       -- no func call
OR UPPER(@s) = 'asdf'                       -- no interest in UPPER
OR GETDATE() = 'asdf'                       -- no interest in GETDATE
BEGIN
    PRINT '?'
END
