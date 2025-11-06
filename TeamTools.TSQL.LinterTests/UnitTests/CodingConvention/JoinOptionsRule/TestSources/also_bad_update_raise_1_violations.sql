UPDATE foo SET -- no join predicate for foo
    last_mod = DEFAULT
FROM bar AS b
INNER JOIN far AS f
    ON f.some_id = b.some_id
WHERE foo.foo_type_code = 'foofoo';
