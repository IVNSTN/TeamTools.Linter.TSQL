CREATE TABLE dbo.foo
(
    descendant_id INT NOT NULL
    , parent_id   INT NOT NULL
    , CONSTRAINT fk FOREIGN KEY (parent_id) REFERENCES dbo.bar(ancestor_id)
);

ALTER TABLE dbo.foo
ADD CONSTRAINT fk2 FOREIGN KEY (descendant_id) REFERENCES dbo.jar(relative_id);


CREATE TABLE #far
(
    title       VARCHAR(100)
    , lastmod   DATETIME2(7) NOT NULL DEFAULT SYSDATETIME()
)
