SELECT *, dbo.har('x') as z
FROM foo
INNER JOIN dbo.bar
    on c = d
INNER JOIN [$(master)].zzz.zar
    ON a = b
