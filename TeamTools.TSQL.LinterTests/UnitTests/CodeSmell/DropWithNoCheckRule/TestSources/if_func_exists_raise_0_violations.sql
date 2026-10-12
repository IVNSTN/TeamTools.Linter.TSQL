IF OBJECT_ID('dbo.foo', 'IF') IS NOT NULL
    DROP FUNCTION dbo.foo;
GO
IF OBJECT_ID('dbo.foo', 'FT') IS NOT NULL
    DROP FUNCTION dbo.foo;
GO
IF OBJECT_ID(('foo'), 'FN') IS NOT NULL
    DROP FUNCTION dbo.foo;
GO
