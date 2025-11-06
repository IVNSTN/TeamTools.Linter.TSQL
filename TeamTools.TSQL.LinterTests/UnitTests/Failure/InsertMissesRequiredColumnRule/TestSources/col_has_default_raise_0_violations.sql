DECLARE @tbl TABLE
(
    id INT,
    title VARCHAR(10) NOT NULL DEFAULT 'foo' -- default will be inserted
)

INSERT @tbl (id)
VALUES (1)
