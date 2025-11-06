CREATE TRIGGER trg on dbo.foo AFTER UPDATE
as
begin
    select 1;
    waitfor delay '00:01';
end;
