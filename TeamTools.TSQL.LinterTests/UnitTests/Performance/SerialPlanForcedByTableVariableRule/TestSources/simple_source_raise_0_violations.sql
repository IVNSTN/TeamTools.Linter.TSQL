-- per-item insert
INSERT INTO @foo(title)
VALUES ('asdf')

-- no from
DELETE @foo

-- from another table variable
INSERT INTO @foo(title)
select * from @bar as b
