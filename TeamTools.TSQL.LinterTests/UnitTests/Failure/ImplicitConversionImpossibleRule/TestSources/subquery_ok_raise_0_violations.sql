DECLARE
    @xml   XML
    , @int INT;

SET @xml = (SELECT f.id FROM dbo.foo AS f FOR XML AUTO);
SET @int = (SELECT TOP (1) 1 FROM dbo.bar AS b WHERE b.parent_id IS NULL);
