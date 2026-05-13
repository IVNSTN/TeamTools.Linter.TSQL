SELECT 1
FROM foo
INNER JOIN bar
ON id = parent_id

INSERT INTO foo(title)
SELECT title
FROM @bar

INSERT INTO foo(title)
    OUTPUT INSERTED.id
    INTO hist.foo_records(inserted_id)
SELECT title
FROM bar
