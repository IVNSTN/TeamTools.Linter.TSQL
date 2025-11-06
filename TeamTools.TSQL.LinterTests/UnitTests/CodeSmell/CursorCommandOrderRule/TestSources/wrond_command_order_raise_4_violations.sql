deallocate cr       -- 1
close cr            -- 2
FETCH NEXT FROM cr into @id -- 3
-- 4 - not deallocated
