SELECT
    1 + 1,
    2 * 2,
    3 / 3,
    4 % 4,
    '1+1' as [2-2]
WHERE
    6 
    - 6 = 0

-- leading and combined operators
SELECT
    -3,
    -COUNT(-a),
    COUNT(*)
WHERE a.b <> z AND x >= y
    OR 1 != 0

set @a += 1

SELECT COUNT(*) FROM foo

-- comment about 1+1 and 2-2

declare @i int = 1 + 1;
