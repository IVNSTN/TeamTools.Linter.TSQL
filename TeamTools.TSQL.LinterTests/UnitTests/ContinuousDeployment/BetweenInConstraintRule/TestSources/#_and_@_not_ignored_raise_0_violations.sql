CREATE TABLE #foo
(
    bar int not null,
    far varchar(10) not null constraint ck check (far between 'a' and 'z'),
    mar datetime,
    constraint pk_foo primary key clustered (bar)
)

ALTER TABLE #bar
ADD constraint ck CHECK (n BETWEEN -100 AND 100)

DECLARE @far TABLE
(
    bar int not null,
    far varchar(10) not null check (far between 'a' and 'z'),
    mar datetime,
    primary key clustered (bar)
)
