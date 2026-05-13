CREATE TABLE foo
(
    id          INT,
    group_id    INT,
    option_a    BIT,
    CONSTRAINT FK_GRP FOREIGN KEY (group_id, option_a) REFERENCES foo_groups (id, option_a)
)
GO

CREATE INDEX IX ON foo(id, option_a)
GO
