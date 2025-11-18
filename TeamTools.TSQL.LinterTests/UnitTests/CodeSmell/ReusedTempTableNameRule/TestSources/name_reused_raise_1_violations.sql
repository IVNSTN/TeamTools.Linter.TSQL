CREATE TABLE #foo
(
    descendant_id INT NOT NULL
    , parent_id   INT NOT NULL
);

DROP TABLE #foo;

-- here
CREATE TABLE #foo
(
    title       VARCHAR(100)
    , lastmod   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
);
