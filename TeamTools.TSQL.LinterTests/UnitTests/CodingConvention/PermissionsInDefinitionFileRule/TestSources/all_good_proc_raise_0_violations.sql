CREATE PROC dbo.Bar as ;
GO
-- misdirected but still not on the object created here
GRANT EXEC on object::foo
to far
GO
