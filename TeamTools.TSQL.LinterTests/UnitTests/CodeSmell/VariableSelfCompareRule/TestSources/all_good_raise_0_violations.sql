IF (@a > @b)
    SELECT 10 AS c1
        , CASE WHEN (@k > 2) THEN @k ELSE 120 END AS c2
        WHERE @b <> @c AND @c < 100
