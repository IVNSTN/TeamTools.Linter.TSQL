-- compatibility level min: 130
CREATE TABLE t
(
    id INT
    , INDEX IX (id)
    ,   INDEX IY (id DESC)
)
GO

CREATE TABLE t
(
    id INT
    , INDEX IX (id)
            , INDEX IY (id DESC)
)
