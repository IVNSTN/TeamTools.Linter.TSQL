insert into bar(a)
select (select 1), null, 'asdf';
