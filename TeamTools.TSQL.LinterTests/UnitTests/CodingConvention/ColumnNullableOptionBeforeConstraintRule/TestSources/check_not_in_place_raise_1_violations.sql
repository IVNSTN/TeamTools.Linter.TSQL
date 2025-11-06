CREATE TABLE foo
(
    bar int           not null identity(1,1)
    , far varchar(10) constraint ck CHECK (convert(varchar(3), far) = 'zar' or far = 'jar') null
    , constraint pk_foo primary key clustered (bar)
)
