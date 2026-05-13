create proc dbo.foo as
begin
    -- this is not a separate object
    create table #t
    (
        id int
    )
end

