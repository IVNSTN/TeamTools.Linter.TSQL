INSERT dbo.Foo WITH (TABLOCKX) (id, name)
SELECT 1, ''

INSERT #bar(id, name)
SELECT 1, ''
