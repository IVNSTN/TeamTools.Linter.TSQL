CREATE TABLE foo
(
    bar int not null default cast('test' as int),   -- 1
    far varchar(10) not null constraint ck check (cast(far as varchar(3)) = 'zar' or far = 'jar'), -- 2
    mar datetime,
    constraint pk_foo primary key clustered (bar)
)

ALTER TABLE foo
ADD constraint df_mar default cast(GETDATE() as DATE) for mar; -- 3
