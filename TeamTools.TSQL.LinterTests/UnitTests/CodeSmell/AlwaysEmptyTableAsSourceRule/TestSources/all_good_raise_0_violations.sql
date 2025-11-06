DECLARE @foo TABLE (id INT)
CREATE TABLE #test (name varchar(100))

insert @foo values (123)

delete o
output deleted.name into #test(name)
from dbo.orders AS o
where canceled = 1
and exists(select 1 from @foo)

select * From #test

create table desttbl(id int)

merge desttbl t
using #test src
on t.id= src.id
when not matched by target then
    insert (id)
    values(src.id);

select * from desttbl;
GO
CREATE TRIGGER trg on dbo.orders AFTER UPDATE
AS
BEGIN
    SELECT * FROM DELETED d inner join INSERTED i
    on i.id = d.id;
END
