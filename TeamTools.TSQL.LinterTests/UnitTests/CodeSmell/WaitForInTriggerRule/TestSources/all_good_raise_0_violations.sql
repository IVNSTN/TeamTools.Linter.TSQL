CREATE TRIGGER trg on dbo.foo AFTER UPDATE
as
begin
    select 1;
end;
