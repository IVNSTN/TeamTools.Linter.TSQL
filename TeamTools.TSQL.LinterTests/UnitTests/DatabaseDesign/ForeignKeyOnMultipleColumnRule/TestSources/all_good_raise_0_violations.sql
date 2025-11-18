DECLARE @foo TABLE
(
    fk INT NOT NULL
)

CREATE TABLE aaa
(
    id int not null primary key,
    b_id int null foreign key references bbb(id),
    c_id int,
    constraint c_fk foreign key (c_id) references ccc(id)
)
