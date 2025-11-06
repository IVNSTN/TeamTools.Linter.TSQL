CREATE INDEX IX ON dbo.foo(title) 
WITH DROP_EXISTING, SORT_IN_TEMPDB  -- both are ON, not default
