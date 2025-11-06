CREATE TRIGGER trg on dbo.foo AFTER UPDATE
as
begin
    THROW 50000, 'test', 1;
end;
