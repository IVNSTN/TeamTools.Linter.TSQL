DECLARE @handle BIGINT, @h BIGINT;

EXEC sp_xml_preparedocument @handle OUT;

EXEC sp_xml_preparedocument @handle OUT; -- 1

EXEC sp_xml_removedocument @handle;

EXEC sp_xml_preparedocument @h OUT; -- 2
