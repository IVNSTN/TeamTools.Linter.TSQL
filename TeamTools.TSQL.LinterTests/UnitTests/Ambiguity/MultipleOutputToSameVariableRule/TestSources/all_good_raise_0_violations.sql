exec my_proc

exec my_proc @foo, 'bar', @dar OUTPUT

exec @res = my_proc
    @p   = @v,
    @var = @foo OUTPUT,
    @dt  = @bar OUTPUT;

exec @res = sp_executesql N'dummy', N'dummy',
    @p   = 'p',
    @var = @foo OUTPUT,
    @dt  = @bar OUTPUT;
