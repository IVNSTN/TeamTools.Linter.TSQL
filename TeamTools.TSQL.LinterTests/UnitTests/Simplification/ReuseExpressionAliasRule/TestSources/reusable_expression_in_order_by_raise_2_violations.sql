SELECT
    (foo.bar * foo.jar - 1) as far
FROM foo
ORDER BY a,
    b,
    ((foo.bar*foo.jar-1)) DESC     -- 1


SELECT
    1 + a
FROM foo
ORDER BY
    (1 + A) -- 2
    , far
