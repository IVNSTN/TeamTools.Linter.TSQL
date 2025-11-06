create trigger trg on dbo.bar after insert
as
begin
    SET STATISTICS TIME OFF
end;
