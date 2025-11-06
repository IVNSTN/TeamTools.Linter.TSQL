CREATE TABLE foo
(
    bar int not null default 1,
    yr INT constraint df_mar DEFAULT DATEPART(YEAR, GETDATE())
    constraint pk_foo primary key clustered (bar)
)
