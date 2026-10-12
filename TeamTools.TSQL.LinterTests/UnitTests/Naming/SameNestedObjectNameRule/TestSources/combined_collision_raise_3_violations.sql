CREATE TABLE dbo.data
(
    data INT NOT NULL,
    CONSTRAINT data PRIMARY KEY CLUSTERED (data)
)
GO
CREATE INDEX data ON data (data)
