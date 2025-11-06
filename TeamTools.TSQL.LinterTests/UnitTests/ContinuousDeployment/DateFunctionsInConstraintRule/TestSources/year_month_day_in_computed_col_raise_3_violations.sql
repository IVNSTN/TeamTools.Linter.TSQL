CREATE TABLE foo
(
    Id    INT NOT NULL
    , dt  DATETIME
    , bar AS YEAR(GETDATE())            -- 1 - YEAR
    , far AS 1 + DAY(dt)                -- 2 - DAY
);
GO

ALTER TABLE foo
ADD jar AS POWER(2, MONTH(GETDATE()))   -- 3 - MONTH
