DECLARE @handle BIGINT;

EXEC sp_xml_preparedocument @handle OUT;

SELECT 1;

EXEC sp_xml_preparedocument @hDoc OUTPUT, @x; -- keep var opening "mixed"

EXEC sp_xml_removedocument @handle;

EXEC sp_xml_removedocument @hDoc;
