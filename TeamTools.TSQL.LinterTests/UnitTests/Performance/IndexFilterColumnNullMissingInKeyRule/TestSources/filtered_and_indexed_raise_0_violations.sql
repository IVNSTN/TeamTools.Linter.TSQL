CREATE INDEX ix ON dbo.foo(id, title)
WHERE title IS NULL;

CREATE INDEX ix ON dbo.foo(id)
INCLUDE(title)
WHERE title IS NULL;
