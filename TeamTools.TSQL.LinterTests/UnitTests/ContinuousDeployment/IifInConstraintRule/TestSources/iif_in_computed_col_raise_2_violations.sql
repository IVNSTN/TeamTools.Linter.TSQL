CREATE TABLE foo
(
    Id    INT NOT NULL
    , bar AS IIF (id > 1, -1, 1)    -- 1
);
GO

ALTER TABLE foo
ADD title AS IIF(bar IS NULL, 'undefined', 'defined') -- 2
