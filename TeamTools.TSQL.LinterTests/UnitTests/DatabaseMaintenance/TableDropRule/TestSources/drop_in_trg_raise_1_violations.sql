create trigger trg on dbo.bar after insert
as
begin
    drop table dbo.main_data
end;
