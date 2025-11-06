CREATE TABLE foo
(
    bar int           not null identity(1,1)
    , mar datetime    DEFAULT convert(DATE, GETDATE()) NOT NULL
    , constraint pk_foo primary key clustered (bar)
)
