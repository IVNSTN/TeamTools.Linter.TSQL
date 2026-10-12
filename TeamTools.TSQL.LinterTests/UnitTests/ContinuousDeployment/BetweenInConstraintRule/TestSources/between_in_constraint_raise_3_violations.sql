CREATE TABLE foo
(
    bar int not null,
    far varchar(10) not null constraint ck check (far between 'a' and 'z'),     -- 1
    mar datetime,
    constraint pk_foo primary key clustered (bar),
    constraint ck_mar check (mar between '19000101' and '20000101')             -- 2
)

ALTER TABLE foo
ADD constraint ck CHECK (bar BETWEEN -100 AND 100)                              -- 3
