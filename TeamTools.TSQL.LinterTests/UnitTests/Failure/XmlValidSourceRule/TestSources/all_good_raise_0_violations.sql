CREATE XML SCHEMA COLLECTION dbo.moex AS
N'<xsd:schema xmlns:xsd="http://www.w3.org/2001/XMLSchema" elementFormDefault="qualified">
</xsd:schema>';
GO
DROP XML SCHEMA COLLECTION dbo.foo;
GO

DECLARE
    @var        XML = '<r/>'
    , @no_init  XML
    , @other    INT;
