CREATE INDEX idx_a on dbo.foo(a)
include(id, a)
GO
CREATE INDEX idx_a on dbo.foo(a)
include(id, c, id)
GO
