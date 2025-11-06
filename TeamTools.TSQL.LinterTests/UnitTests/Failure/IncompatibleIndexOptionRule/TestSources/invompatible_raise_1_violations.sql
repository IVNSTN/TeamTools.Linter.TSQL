CREATE INDEX IX
    ON dbo.foo (col)
    WHERE (col2 IS NOT NULL)
    WITH (IGNORE_DUP_KEY = ON); -- not unique and filtered
GO
