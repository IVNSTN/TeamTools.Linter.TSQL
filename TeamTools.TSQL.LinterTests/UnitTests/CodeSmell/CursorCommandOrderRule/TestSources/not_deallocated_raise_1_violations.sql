DECLARE cr CURSOR READ_ONLY FOR
    SELECT 1;
OPEN cr;
FETCH NEXT FROM cr into @id

WHILE @@FETCH_STATUS = 0
BEGIn
    fetch next from cr into @id
END

close cr
