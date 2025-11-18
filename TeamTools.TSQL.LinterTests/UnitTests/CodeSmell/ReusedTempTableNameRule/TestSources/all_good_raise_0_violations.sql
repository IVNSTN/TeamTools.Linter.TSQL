CREATE TABLE #foo
(
    descendant_id INT NOT NULL
    , parent_id   INT NOT NULL
);

DECLARE @foo TABLE
(
    title       VARCHAR(100)
    , lastmod   DATETIME2(7)
);

CREATE TABLE dbo.foo
(
    descendant_id INT NOT NULL
    , parent_id   INT NOT NULL
);
