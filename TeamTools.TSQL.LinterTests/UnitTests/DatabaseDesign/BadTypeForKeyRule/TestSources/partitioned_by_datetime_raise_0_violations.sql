CREATE TABLE dbo.SensorsData
(
    ServiceID         INT          NOT NULL
    , SensorID        INT          NOT NULL
    , dTime           BIGINT       NOT NULL
    , SensorTime      DATETIME2(7) NOT NULL
    , CONSTRAINT PK_dbo_SensorsData PRIMARY KEY CLUSTERED (SensorTime ASC, SensorID ASC, ServiceID ASC) WITH (DATA_COMPRESSION = PAGE) ON sh_dt2_20140101_20251231([SensorTime])
);
GO
