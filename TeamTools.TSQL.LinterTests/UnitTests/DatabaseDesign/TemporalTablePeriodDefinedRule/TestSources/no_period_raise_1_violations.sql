CREATE TABLE foo.bar
(
    code                VARCHAR(100)  NOT NULL
    , descr             VARCHAR(4000) NOT NULL
    , CONSTRAINT PK_foo_bar PRIMARY KEY NONCLUSTERED (code)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = foo.bar_history));
GO
