CREATE TABLE asdf.foo
(
    some_id          INT          NOT NULL
    , sys_start_time DATETIME2(7) GENERATED ALWAYS AS ROW START NOT NULL
    , sys_end_time   DATETIME2(7) GENERATED ALWAYS AS ROW END NOT NULL
    , CONSTRAINT PK_asdf_foo PRIMARY KEY CLUSTERED (some_id ASC)
    , PERIOD FOR SYSTEM_TIME(sys_start_time, sys_end_time)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = asdf.foo_history, DATA_CONSISTENCY_CHECK = ON));
GO

CREATE TABLE asdf.foo
(
    some_id          INT          NOT NULL
)
GO

-- here both are dbo
CREATE TABLE foo
(
    some_id          INT          NOT NULL
    , sys_start_time DATETIME2(7) GENERATED ALWAYS AS ROW START NOT NULL
    , sys_end_time   DATETIME2(7) GENERATED ALWAYS AS ROW END NOT NULL
    , CONSTRAINT PK_dbo_foo PRIMARY KEY CLUSTERED (some_id ASC)
    , PERIOD FOR SYSTEM_TIME(sys_start_time, sys_end_time)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = foo_history, DATA_CONSISTENCY_CHECK = ON));
GO
