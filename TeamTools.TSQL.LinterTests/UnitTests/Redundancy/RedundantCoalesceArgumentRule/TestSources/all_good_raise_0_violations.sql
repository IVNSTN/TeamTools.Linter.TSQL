SELECT COALESCE(@a, CASE WHEN 1=0 THEN 0 ELSE @b END, c.d, 'asdf')
from c
