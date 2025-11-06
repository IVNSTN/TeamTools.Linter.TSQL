CREATE TRIGGER trg on dbo.foo AFTER UPDATE
as
begin
    raiserror('test', 16, 1);
end;
