-- compatibility level min: 130
CREATE TABLE foo
(
    id int not null primary key,
    b_id int null foreign key references bbb(id),
    c_id int,
    index ix (a ASC, b DESC, c ASC)
)
