DECLARE cr CURSOR READ_ONLY FOR
    SELECT 1;
OPEN cr;
FETCH NEXT FROM cr into @id

WHILE @@FETCH_STATUS = 0
BEGIn
    fetch next from cr into @id
END

close cr
deallocate cr

-- name reuse
DECLARE cr CURSOR READ_ONLY FOR
    SELECT 1;
OPEN cr;
FETCH NEXT FROM cr into @id

WHILE @@FETCH_STATUS = 0
BEGIn
    fetch next from cr into @id
END

close cr
deallocate cr
GO

-- cursor as var
DECLARE @cr CURSOR

SET @cr = CURSOR FAST_FORWARD FOR
    SELECT 1;

OPEN @cr;
FETCH NEXT FROM @cr into @id

WHILE @@FETCH_STATUS = 0
BEGIn
    fetch next from @cr into @id
END

CLOSE @cr;
DEALLOCATE @cr;
GO
