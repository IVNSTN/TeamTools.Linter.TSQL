SELECT * FROM sysobjects -- 1

SELECT * FROM syscolumns c -- 2
JOIN sys.tables t
ON c.object_id = t.object_id
