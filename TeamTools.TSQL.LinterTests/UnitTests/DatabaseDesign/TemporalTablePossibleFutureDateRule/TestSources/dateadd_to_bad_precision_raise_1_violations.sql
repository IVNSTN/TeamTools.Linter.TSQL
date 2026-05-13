CREATE TABLE foo.bar
(
    code                VARCHAR(100)  NOT NULL
    , descr             VARCHAR(4000) NOT NULL
    -- only START date is reported by the rule
    , sys_start_time    DATETIME2(2)  GENERATED ALWAYS AS ROW START HIDDEN NOT NULL CONSTRAINT DF_foo_bar_sys_start_time
        DEFAULT DATEADD(DAY, 1, SYSUTCDATETIME())   -- here
    , sys_end_time      DATETIME2(1)  GENERATED ALWAYS AS ROW END HIDDEN NOT NULL
    , PERIOD FOR SYSTEM_TIME(sys_start_time, sys_end_time)
    , CONSTRAINT PK_foo_bar PRIMARY KEY NONCLUSTERED (code)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = foo.bar_history));
GO
