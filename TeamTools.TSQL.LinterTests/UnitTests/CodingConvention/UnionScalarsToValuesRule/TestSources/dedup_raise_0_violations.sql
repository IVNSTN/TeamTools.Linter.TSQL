SELECT col1, col2, col3
FROM
(
    SELECT
        @var1_1
        , @var1_2
        , @var1_3
    UNION
    SELECT
        @var2_1
        , @var2_2
        , @var1_3

) AS cmp (col1, col2, col3)
