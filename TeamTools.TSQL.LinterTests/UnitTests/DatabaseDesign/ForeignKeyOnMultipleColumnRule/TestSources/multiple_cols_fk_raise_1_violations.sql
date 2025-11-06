CREATE TABLE aaa
(
    id int not null primary key,
    b_id int null,
    c_id int,
    constraint c_fk foreign key (b_id, c_id) references ccc(id, b_id)
)
