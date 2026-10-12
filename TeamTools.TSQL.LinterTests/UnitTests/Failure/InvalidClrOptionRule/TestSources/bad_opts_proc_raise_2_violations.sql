CREATE PROCEDURE dbo.bar (@arg INT)
WITH RECOMPILE                                  -- 1
FOR REPLICATION                                 -- 2
AS
EXTERNAL NAME proj.dll.func;
GO
