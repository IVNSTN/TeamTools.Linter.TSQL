CREATE TABLE #expected
(
    doc_id  INT NOT NULL PRIMARY KEY
)
GO

CREATE TABLE dbo.foo
(
    major_id        BIGINT          NOT NULL
    , secondary_id  INT             NOT NULL
    , CONSTRAINT pk_foo PRIMARY KEY CLUSTERED (major_id ASC)
    , INDEX ix_secondary_id NONCLUSTERED (major_id, secondary_id DESC)
);
GO
