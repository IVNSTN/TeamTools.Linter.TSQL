SELECT 1
FROM dbo.foo AS f
INNER JOIN scm.bar AS b
    ON b.value IN (f.val_1, f.val_2)
