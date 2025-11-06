SELECT
    MAX(t.distance) OVER(PARTITION BY t.zone) as mdist
