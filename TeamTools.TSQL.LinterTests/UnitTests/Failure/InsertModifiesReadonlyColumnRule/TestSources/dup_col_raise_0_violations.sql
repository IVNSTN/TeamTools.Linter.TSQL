DECLARE @tbl TABLE
(
    id INT,
    id INT, -- intentional dup
    title VARCHAR(10) NOT NULL DEFAULT 'foo'
)

INSERT @tbl
EXEC dbo.bar -- no idea how many columns will come out
