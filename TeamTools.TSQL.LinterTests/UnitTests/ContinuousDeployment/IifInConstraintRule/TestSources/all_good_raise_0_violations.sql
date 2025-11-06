CREATE TABLE foo
(
    bar int not null default 1,
    yr INT constraint df_mar DEFAULT CASE WHEN 1=1 THEN 2 ELSE 3 END,
    constraint pk_foo primary key clustered (bar),
    bar AS CASE WHEN id > 1 THEN -1 ELSE 1 END,
    CONSTRAINT ck_foo CHECK (0 = CASE WHEN 1=1 THEN 2 ELSE 3 END)
)
