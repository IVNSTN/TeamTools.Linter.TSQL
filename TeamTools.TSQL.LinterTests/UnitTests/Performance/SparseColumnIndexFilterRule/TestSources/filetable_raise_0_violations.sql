-- compatibility level min: 110
-- no cols, no index
CREATE TABLE files.docs_storage AS FILETABLE FILESTREAM_ON document_filestream_group
GO
