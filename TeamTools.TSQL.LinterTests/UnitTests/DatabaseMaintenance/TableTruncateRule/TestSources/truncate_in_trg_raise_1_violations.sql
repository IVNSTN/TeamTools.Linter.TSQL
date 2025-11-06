create trigger trg on dbo.bar after insert
as
begin
    truncate table dbo.main_data
end;
