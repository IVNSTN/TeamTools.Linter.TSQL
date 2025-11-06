INSERT dbo.foo(a, b) VALUES ('bar', 'zar')

INSERT dbo.foo(a, b)
SELECT bar, far
FROM zar
WHERE dar <> gar
ORDER BY mar, nar, jar

MERGE x
USING y
ON x.a = y.b
WHEN MATCHED THEN DELETE
WHEN NOT MATCHED THEN INSERT (z) VALUES (zz);

DELETE far
FROM far
WHERE nar != zar;

UPDATE far set jar = par
FROM far
WHERE nar != zar;

SELECT 0 FROM zzz WHERE a= b;

SELECT a
FROM b
WHERE a IS NOT NULL
GROUP BY a
HAVING COUNT(a) > 0
ORDER BY a
