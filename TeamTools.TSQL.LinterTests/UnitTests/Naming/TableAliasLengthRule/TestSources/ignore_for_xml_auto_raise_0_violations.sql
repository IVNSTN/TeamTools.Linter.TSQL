select t.*
from dbo.bar t 
FOR XML AUTO

select t.*
from #bar t 
FOR XML AUTO, ROOT('r')

SELECT @package = (
    select t.*
    from @bar t
    FOR XML AUTO, XMLSCHEMA, TYPE
)
