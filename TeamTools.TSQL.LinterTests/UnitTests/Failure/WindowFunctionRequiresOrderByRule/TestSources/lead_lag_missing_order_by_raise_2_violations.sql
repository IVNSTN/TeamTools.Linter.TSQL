SELECT
    LEAD(t.step) OVER (PARTITION BY t.distance) AS next_step
    , LAG(t.step) OVER () AS prior_step;
