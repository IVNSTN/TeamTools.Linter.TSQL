CREATE TABLE #foo
(
    bar     VARCHAR(10) NOT NULL DEFAULT '<NULL>'
    , far   NCHAR CONSTRAINT DF_foo_far DEFAULT ('  null  ')
)
