IF NOT (@a = @b)
    SELECT CASE WHEN (NOT ((@c > @d))) THEN 1 ELSE 0 END AS res
