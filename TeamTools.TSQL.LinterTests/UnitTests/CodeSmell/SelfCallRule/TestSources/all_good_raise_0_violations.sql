create proc dbo.foo
as
begin
    exec sp_help 'sys.objects'

    return 1
end
GO

create proc dbo.foo;3
as
begin
    exec dbo.foo;2  -- different number

    return 1
end
GO

CREATE FUNCTION foo.bar (@value int)
RETURNS INT
AS
BEGIN
    SET @value = @value + 1;

    RETURN @value;
END
GO
