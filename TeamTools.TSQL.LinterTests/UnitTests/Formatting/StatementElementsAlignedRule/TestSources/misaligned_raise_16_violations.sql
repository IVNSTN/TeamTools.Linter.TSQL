INSERT dbo.foo(a, b)
    SELECT bar, far
        FROM zar
            WHERE dar <> gar
                ORDER BY mar, nar, jar

MERGE x
    USING (SELECT * FROM y) as y
        ON x.a = y.b
            WHEN MATCHED AND 1=1 THEN DELETE
                WHEN NOT MATCHED THEN INSERT (z) VALUES (zz);

DELETE far
    FROM far
        WHERE nar != zar;

UPDATE far set jar = par
    FROM far
        WHERE nar != zar;

SELECT a
    FROM b
        WHERE a IS NOT NULL
            GROUP BY a
                HAVING COUNT(a) > 0
                    ORDER BY a
