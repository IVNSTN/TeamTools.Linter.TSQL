create procedure dbo.foo
as
begin
    if 1=0
        return;
    else
        return 1;
end;
go

create trigger dbo.bar
on dbo.zar
after delete as
begin
    return;
end;
go

create function dbo.far()
returns int
as
begin
    return 3;
end;
go

create function dbo.tar()
returns @t table (id int)
as
begin
    return;
end;
