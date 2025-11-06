if exists(select 1)
    return;

insert #tbl
SELECT TOP(1) 1
