-- INSERT
insert into a(title)
    output DELETED.*            -- 1
select title
from src

insert into a(title)
    output DELETED.id           -- 2
    into del.hist(id)
select title
from src

-- DELETE
delete d
    output INSERTED.id          -- 3
from tbl as d
where category_id = 1

delete from d
    output INSERTED.*           -- 4
    into history.del_category
from tbl as d
where category_id = 1
