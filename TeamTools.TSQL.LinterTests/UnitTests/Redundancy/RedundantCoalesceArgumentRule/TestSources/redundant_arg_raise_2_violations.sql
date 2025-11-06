-- NULL and 'asdf'
SELECT COALESCE(NULL, @a, c.d, 'asdf', 213)
from c
