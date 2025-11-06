create table test.test
(
    id int not null,
    val int null,
    summary as (1 + 2));

declare @foo table (id int not null);
create table #bar (name varchar(10) null);
