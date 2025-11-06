select top 10 *
from dbo.foo
where freetext(title, 'asdf')

select top 10 *
from dbo.foo
INNER JOIN FREETEXTTABLE(dbo.bar, title, 'asdf', LANGUAGE N'english', 7) t
ON t.[KEY] = foo.id
