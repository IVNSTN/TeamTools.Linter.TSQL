WITH b AS
(
    SELECT * FROM a AS b
)
SELECT
    b.ID,
    (SELECT TOP 1 last_upd FROM b WHERE id = parent_Id ORDER BY last_upd DESC) AS last_upd,
    (SELECT c.code FROM b AS c FOR XML PATH) AS codes
FROM b
ORDER BY d.ID
