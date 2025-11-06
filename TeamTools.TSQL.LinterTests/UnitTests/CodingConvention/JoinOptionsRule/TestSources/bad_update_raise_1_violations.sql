UPDATE foo
SET last_mod = DEFAULT
FROM bar
WHERE foo.id = bar.id;
