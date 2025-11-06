exec @var = sp_executesql N'dummy', N'dummy',
    @var = @var OUTPUT,
    @dt  = @var OUTPUT;

exec sp_executesql N'dummy', N'dummy',
    @var = @var OUTPUT,
    @dt  = @var OUTPUT;
