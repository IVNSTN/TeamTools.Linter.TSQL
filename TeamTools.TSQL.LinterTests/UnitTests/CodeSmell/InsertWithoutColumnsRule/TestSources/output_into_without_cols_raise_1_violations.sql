update t set
    a = b
    output inserted (a, b)
    into tmp.bar
from dbo.foo as t;
