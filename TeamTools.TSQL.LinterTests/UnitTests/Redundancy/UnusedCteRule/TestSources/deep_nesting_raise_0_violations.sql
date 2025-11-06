;WITH recursion AS
(
    SELECT f.id, f.parent_id
    FROM dbo.foo
    WHERE parent_id IS NULL
    UNION ALL
    SELECT f.id, f.parent_id
    FROM dbo.foo f
    INNER JOIN recursion r
    ON f.parent_id = r.parent_id
), child AS (
    SELECT * FROM recursion
), grand_child AS (
    SELECT * FROM child
)
SELECT * FROM grand_child
