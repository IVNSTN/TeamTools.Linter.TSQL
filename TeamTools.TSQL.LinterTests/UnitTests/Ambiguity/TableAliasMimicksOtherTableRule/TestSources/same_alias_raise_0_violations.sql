-- another rule handles this ambiguity
SELECT f.*
FROM foo AS f
INNER JOIN far AS f
    on foo.id = far.id
ORDER BY f.id
