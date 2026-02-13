-- compatibility level min: 110
SELECT IIF(@a > 1, (@b

    + 42),
    @b+42)
