-- there are 36 violations, 7 is the max limit per batch
set nocount on;
declare @a int;
if 1 <> 0
begin
    select top 1 1 as z from foo as f
    where bar <> cast(zar as date)
    order by case when upper(a) = a then 1 else 0 end;
end;
begin try
    execute as user = 'admin';
    exec dbo.test;
    revert;
end try
begin catch
    rollback tran;
end catch;
return;
