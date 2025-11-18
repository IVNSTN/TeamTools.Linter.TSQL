CREATE TABLE #foo
(
    id INT NOT NULL PRIMARY KEY,
    rv ROWVERSION NOT NULL          -- 1
)

DECLARE @bar TABLE
(
    id INT NOT NULL PRIMARY KEY,
    rv ROWVERSION NOT NULL          -- 2
)

ALTER TABLE #far
    ADD rv ROWVERSION               -- 3
GO
