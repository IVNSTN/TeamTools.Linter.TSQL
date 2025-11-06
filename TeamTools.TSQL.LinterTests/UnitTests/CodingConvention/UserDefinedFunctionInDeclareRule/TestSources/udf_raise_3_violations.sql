declare @var int = (select dbo.max_var(1)),
    @dt DATETIME = dbo.f_today(),
    @int INT = my.f_last_update()
