INSERT INTO foo(title)
OUTPUT INSERTED.id          -- here
SELECT title
FROM bar
