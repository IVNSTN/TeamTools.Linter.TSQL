WITH cte AS
(
    SELECT
        foo.title,
        NULL as start_time  -- here
    FROM foo
)
SELECT * FROM cte
