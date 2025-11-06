DELETE t
    OUTPUT
        GETDATE(),
        1,
        @var
    INTO dbo.log
FROM dbo.tbl

DELETE t
    OUTPUT
        GETDATE(),
        1,
        @var
FROM dbo.tbl
