CREATE TABLE foo
(
    Id    INT NOT NULL
    , bar AS ((1 + CAST('1' AS INT)))   -- 1
);
GO

ALTER TABLE foo
ADD today AS CAST(GETDATE() AS DATE);   -- 2
