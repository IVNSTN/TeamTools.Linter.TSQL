create proc foo
as
begin
BEGIN TRAN
SELECT 1
ROLLBACK TRAN
end
