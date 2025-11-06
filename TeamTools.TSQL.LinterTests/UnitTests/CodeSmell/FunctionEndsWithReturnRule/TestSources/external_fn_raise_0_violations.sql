CREATE FUNCTION dbo.my_split_fn (@str VARCHAR(4000), @delim CHAR(1))
RETURNS TABLE (ID INT NULL)
AS
    EXTERNAL NAME StringAggregates.foo.bar;
GO
