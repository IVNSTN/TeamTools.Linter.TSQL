-- compatibility level min: 130
/* FIXME : columnstore table should be ignored
CREATE TABLE dbo.foo
(
    flag     BIT      NULL
    , prefix NCHAR(2) NULL
);
*/
GO

CREATE CLUSTERED COLUMNSTORE INDEX IX
    ON dbo.foo
    WITH (DATA_COMPRESSION = COLUMNSTORE_ARCHIVE);
GO
