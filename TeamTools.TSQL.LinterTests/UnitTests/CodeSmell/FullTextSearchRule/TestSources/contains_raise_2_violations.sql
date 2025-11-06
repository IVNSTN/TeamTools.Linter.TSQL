select top 10 *
from dbo.foo
where CONTAINS(title, 'asdf')

select top 10 *
from dbo.foo
INNER JOIN CONTAINSTABLE(dbo.bar, title, 'asdf', LANGUAGE N'english', 7) t
ON t.[KEY] = foo.id
