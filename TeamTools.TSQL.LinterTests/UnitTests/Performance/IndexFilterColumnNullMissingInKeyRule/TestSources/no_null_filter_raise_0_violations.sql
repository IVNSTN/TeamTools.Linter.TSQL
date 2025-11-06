CREATE INDEX ix ON dbo.foo(id, title);

CREATE INDEX ix ON dbo.foo(id)
WHERE title IS NOT NULL;
