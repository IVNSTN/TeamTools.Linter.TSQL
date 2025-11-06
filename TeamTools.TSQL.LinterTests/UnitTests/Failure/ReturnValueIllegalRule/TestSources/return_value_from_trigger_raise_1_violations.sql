create trigger dbo.bar
on dbo.zar
after delete as
begin
    return 1;
end;
