-- rule reports violations aggregated thus I had to split one bad query
-- to multiple ones with a single violation per query
UPDATE t SET
    title = (SELECT miple FROM dbo.zar z -- 1
        WHERE z.id = t.parent_id)
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = b.parent_id
WHERE t.title LIKE 'asdf%'
    AND EXISTS(SELECT 1 FROM dbo.jar j WHERE j.name = t.diple)
GO
UPDATE t SET
    title = (SELECT t.miple FROM dbo.zar z
        WHERE z.id = parent_id) -- 2
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = b.parent_id
WHERE t.title LIKE 'asdf%'
    AND EXISTS(SELECT 1 FROM dbo.jar j WHERE j.name = t.diple)
GO
UPDATE t SET
    title = 'A'
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = b.parent_id
WHERE t.title LIKE 'asdf%'
    AND EXISTS(SELECT 1 FROM dbo.jar j WHERE j.name = diple) -- 3
GO

DELETE t
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = (SELECT parent_id) -- 4
WHERE t.title LIKE 'asdf%'
    AND NOT EXISTS(SELECT 1 FROM dbo.jar j WHERE j.name = t.kriple)
GO

DELETE t
FROM dbo.foo AS t
INNER JOIN dbo.bar AS b
    ON t.id = b.id
WHERE t.title LIKE 'asdf%'
    AND NOT EXISTS(SELECT 1 FROM dbo.jar j WHERE j.name = kriple) -- 5
GO
