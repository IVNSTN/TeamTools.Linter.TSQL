SELECT *
FROM a
INNER JOIN b
    ON a.a = b.b
LEFT JOIN c
    ON c.c = b.b
RIGHT JOIN d
    ON d.d = a.a
FULL JOIN e
    ON e.e = c.c
CROSS JOIN f
OUTER APPLY (select 1 from g) as g
CROSS APPLY (select 1 from h) as h;
