-- compatibility level min: 150
CREATE CLUSTERED COLUMNSTORE INDEX IX1 ON dbo.foo
WITH (ONLINE = ON)          -- 1 - not applicable to COLUMNSTORE index

CREATE INDEX IX2 ON dbo.foo (id)
WITH (RESUMABLE = ON)       -- 2 - missing ONLINE

CREATE INDEX IX3 ON dbo.foo (id)
WITH (MAX_DURATION = 10)    -- 3 - missing RESUMABLE
