-- INSERT
-- no output
insert into a(title)
select title
from src

insert into a(title)
    output inserted.lastmod
select title
from src

insert into a(title)
    output inserted.id, inserted.lastmod
    into #mod (id, lastmod)
select title
from src

-- DELETE
-- no output
delete src
where category_id = 1

delete from d
    output deleted.id
from tbl as d
where category_id = 1

delete from d
    output deleted.id, GETDATE()
    into history.del_category(id, del_dt)
from tbl as d
where category_id = 1

-- UPDATE
-- deleted
update d set
    lastmod = GETDATE()
    output deleted.id
    into history.upd_category(id)
from tbl as d
where category_id = 1

-- inserted
update d set
    lastmod = GETDATE()
    output inserted.lastmod
    into history.upd_category(lastmod)
from tbl as d
where category_id = 1

-- both
update d set
    title = @new_title
    output inserted.title, deleted.title
    into history.upd_category(new_title, old_title)
from tbl as d
where category_id = 1
