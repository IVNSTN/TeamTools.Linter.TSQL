SELECT *
FROM a
JOIN b
    ON a.a = b.b
LEFT OUTER JOIN c
    ON c.c = b.b
RIGHT OUTER JOIN d
    ON d.d = a.a
FULL OUTER JOIN e
    ON e.e = c.c;
