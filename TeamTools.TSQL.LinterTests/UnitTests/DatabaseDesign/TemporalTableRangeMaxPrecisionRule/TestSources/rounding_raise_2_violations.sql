CREATE TABLE foo.bar
(
    code                VARCHAR(100)  NOT NULL
    , descr             VARCHAR(4000) NOT NULL
    -- here
    , sys_start_time    DATETIME2(3)  GENERATED ALWAYS AS ROW START HIDDEN NOT NULL CONSTRAINT DF_foo_bar_sys_start_time DEFAULT SYSUTCDATETIME()
    , sys_end_time      DATETIME2(1)  GENERATED ALWAYS AS ROW END HIDDEN NOT NULL CONSTRAINT DF_foo_bar_sys_end_time DEFAULT CONVERT(DATETIME2(7), '9999-12-31 23:59:59.9999999')
    , PERIOD FOR SYSTEM_TIME(sys_start_time, sys_end_time)
    , CONSTRAINT PK_foo_bar PRIMARY KEY NONCLUSTERED (code)
)
WITH (SYSTEM_VERSIONING = ON (HISTORY_TABLE = foo.bar_history));
GO
