DECLARE @i INT

-- FIXME : if var was never assigned it should give violation as well
-- explicit assignment must not be required
SET @i = NULL
SELECT @i + 1 -- 1

DECLARE @foo VARCHAR(3)= 'ABC'

SELECT NULLIF(@foo, 'ABC') + 'DEF' -- 2
