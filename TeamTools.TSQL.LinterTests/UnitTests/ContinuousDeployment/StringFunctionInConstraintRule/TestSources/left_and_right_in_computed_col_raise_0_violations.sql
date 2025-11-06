-- it comes that SSDT does not produce everlasting diff for computed cols in these cases
CREATE TABLE foo
(
    Id      INT NOT NULL
    , title VARCHAR(10)
    , bar   AS LEFT(title, 5)
);
GO

ALTER TABLE foo
ADD zar AS (RIGHT(title, 5))
