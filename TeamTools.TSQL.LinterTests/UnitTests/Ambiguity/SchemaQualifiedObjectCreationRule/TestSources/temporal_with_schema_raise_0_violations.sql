-- compatibility level min: 130
CREATE TABLE dbo.foo2
(
    some_id          INT          NOT NULL
    , sys_start_time DATETIME2(7) GENERATED ALWAYS AS ROW START NOT NULL
    , sys_end_time   DATETIME2(7) GENERATED ALWAYS AS ROW END NOT NULL
    , CONSTRAINT PK_dbo_foo PRIMARY KEY CLUSTERED (some_id ASC)
    , PERIOD FOR SYSTEM_TIME(sys_start_time, sys_end_time)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = dbo.foo_history, DATA_CONSISTENCY_CHECK = ON));
GO
