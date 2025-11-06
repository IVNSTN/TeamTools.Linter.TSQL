CREATE TABLE #tbl (
    bad_id VARCHAR(MAX) PRIMARY KEY -- 1
)

CREATE TABLE #test
(
    uid  UNIQUEIDENTIFIER
    , dt DATETIME
    , PRIMARY KEY CLUSTERED (uid, dt) -- 2
)
