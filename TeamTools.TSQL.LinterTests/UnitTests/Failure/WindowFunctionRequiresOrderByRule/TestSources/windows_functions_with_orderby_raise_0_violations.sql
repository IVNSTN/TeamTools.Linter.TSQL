SELECT
    ROW_NUMBER() OVER (ORDER BY t.name) AS rn
    , RANK() OVER (ORDER BY t.speed) AS rnk
    , DENSE_RANK() OVER (ORDER BY t.speed) AS drnk
    , NTILE() OVER (ORDER BY t.speed) AS ntl
    , LEAD(t.step) OVER (ORDER BY t.distance) AS next_step
    , LAG(t.step) OVER (ORDER BY t.distance) AS prior_step
