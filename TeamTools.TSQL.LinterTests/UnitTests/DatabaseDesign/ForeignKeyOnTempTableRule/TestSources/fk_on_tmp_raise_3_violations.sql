CREATE TABLE #foo
(
    descendant_id INT NOT NULL
    , parent_id   INT NOT NULL
    , CONSTRAINT fk FOREIGN KEY (parent_id) REFERENCES dbo.bar(ancestor_id) -- 1
);

ALTER TABLE #foo
ADD CONSTRAINT fk2 FOREIGN KEY (descendant_id) REFERENCES dbo.jar(relative_id); -- 2

ALTER TABLE dbo.bar
ADD CONSTRAINT fk2 FOREIGN KEY (descendant_id) REFERENCES #jar(relative_id);    -- 3
