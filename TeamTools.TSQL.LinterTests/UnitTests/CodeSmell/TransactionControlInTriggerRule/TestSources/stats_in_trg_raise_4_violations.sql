create trigger trg on dbo.bar after insert
as
begin
    BEGIN TRAN
    SAVE TRAN my_tran
    ROLLBACK TRAN
    commit tran
end;
