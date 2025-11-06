SELECT
    ROW_NUMBER() OVER (PARTITION BY t.name) AS rn
    , RANK() OVER (PARTITION BY t.speed) AS rnk
    , DENSE_RANK() OVER (PARTITION BY t.speed) AS drnk
    , NTILE() OVER (PARTITION BY t.speed) AS ntl;
