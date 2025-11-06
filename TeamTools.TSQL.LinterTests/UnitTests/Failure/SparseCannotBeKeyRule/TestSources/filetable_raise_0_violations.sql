-- compatibility level min: 110
CREATE TABLE files.docs_storage AS FILETABLE FILESTREAM_ON document_filestream_group
WITH (FILETABLE_COLLATE_FILENAME = Cyrillic_General_CI_AS, FILETABLE_DIRECTORY = N'docs_storage');
GO
