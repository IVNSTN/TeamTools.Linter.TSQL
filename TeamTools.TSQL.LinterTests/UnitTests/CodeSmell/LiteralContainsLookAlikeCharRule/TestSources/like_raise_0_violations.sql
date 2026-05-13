IF @nick_name NOT LIKE '[A-Za-zА-Яа-я]%'
    PRINT 1

SELECT 1
FROM foo
WHERE title LIKE @pattern -- not a literal

SELECT 1
FROM foo
WHERE title LIKE 'asdf%'  -- no char mix
