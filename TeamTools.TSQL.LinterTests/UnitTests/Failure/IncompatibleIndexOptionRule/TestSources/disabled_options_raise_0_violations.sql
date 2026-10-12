CREATE INDEX IX
    ON dbo.foo (col)
    WHERE (col2 IS NOT NULL)
    WITH (IGNORE_DUP_KEY = OFF); -- not unique and filtered - BUT OFF
GO

CREATE NONCLUSTERED INDEX IX
    ON dbo.archive (db_date ASC, res_code ASC, doc_id ASC)
    WITH (PAD_INDEX = OFF); -- no FILLFACTOR - BUT OFF
GO
