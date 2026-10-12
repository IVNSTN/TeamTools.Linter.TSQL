-- compatibility level min: 130
DECLARE @t TABLE
(
    id INT
)

CREATE TABLE t
(
    id INT
    , INDEX IX (id)
)
