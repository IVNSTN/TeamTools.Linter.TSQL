CREATE TABLE foo
(
    bar int           identity(1,1) not null
    , far varchar(10) null constraint ck CHECK (convert(varchar(3), far) = 'zar' or far = 'jar')
    , zar int
    , mar datetime    NOT NULL DEFAULT convert(DATE, GETDATE())
    , lar varchar(10) DEFAULT ''
    , comp            AS (CASE WHEN (zar > bar) THEN bar ELSE zar END)
    , constraint pk_foo primary key clustered (bar)
)
