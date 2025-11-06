DECLARE
    @xml   XML
    , @int INT;

SET @int = (SELECT f.id FROM dbo.foo AS f FOR XML AUTO, TYPE);
SET @xml = (SELECT TOP (1) 1 FROM dbo.bar AS b WHERE b.parent_id IS NULL);
