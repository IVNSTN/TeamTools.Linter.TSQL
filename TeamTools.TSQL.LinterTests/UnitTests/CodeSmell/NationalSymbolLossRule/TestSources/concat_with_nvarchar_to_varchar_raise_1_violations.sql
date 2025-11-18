create proc dbo.foo
    @arg NVARCHAR(100)
as
begin
    declare @var VARCHAR(1000)

    set @var = -- here
        'asdf' + @arg + ';'
end
