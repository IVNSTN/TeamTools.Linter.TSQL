DECLARE @a INT;

INSERT tbl
EXEC my_proc

SET @a = 1; -- always sets ROWCOUNT to 1

SELECT @@ROWCOUNT;
