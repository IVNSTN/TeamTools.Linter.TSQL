create table #foo
(
    bar int not null default 1,
    primary key (bar)
)

alter table #foo add title varchar(100)
