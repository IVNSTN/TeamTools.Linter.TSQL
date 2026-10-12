UPDATE b SET
    lasdmod = GETDATE()
FROM #bar b
INNER JOIN far f
    ON f.id = b.parent_id

UPDATE f SET
    lasdmod = GETDATE()
FROM bar b
INNER JOIN #far f
    ON f.id = b.parent_id

DELETE b
FROM #bar b
INNER JOIN far f
    ON f.id = b.parent_id

DELETE f
FROM #bar b
INNER JOIN @far f
    ON f.id = b.parent_id
