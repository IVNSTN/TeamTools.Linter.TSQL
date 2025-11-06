SELECT 1
UNION ALL
SELECT @a -- no where, no from
UNION
SELECT (SELECT ((0)));
