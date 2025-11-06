OPEN cr;    -- 1

deallocate cr -- 2
FETCH NEXT FROM cr into @id -- 3

close cr
deallocate cr
