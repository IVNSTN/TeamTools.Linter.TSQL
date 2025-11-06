CREATE TABLE #foo
(
    bar int not null default cast('test' as int),
    far varchar(10) not null constraint ck check (cast(far as varchar(3)) = 'zar' or far = 'jar'),
    mar datetime constraint df_mar default cast(GETDATE() as DATE)
    constraint pk_foo primary key clustered (bar)
)

DECLARE @foo TABLE
(
    bar int not null default cast('test' as int),
    far varchar(10) not null check (cast(far as varchar(3)) = 'zar' or far = 'jar'),
    mar datetime default cast(GETDATE() as DATE),
    primary key clustered (bar)
)
