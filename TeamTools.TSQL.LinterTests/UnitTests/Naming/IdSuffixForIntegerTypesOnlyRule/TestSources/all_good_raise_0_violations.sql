CREATE TABLE foo
(
    bar_id  int    not null identity(1,1)
    , IdFar BIGINT not null
    , zar   int
    , Identifier UNIQUEIDENTIFIER
    , comp  AS id * 2
    , id SMALLINT
    , constraint pk_foo primary key clustered (bar)
)

DECLARE @foo TABLE
(
    bar_id  int    not null identity(1,1)
    , IdFar BIGINT not null
    , zar   int
    , Identifier UNIQUEIDENTIFIER
    , id SMALLINT
    , comp  AS id * 2
)
