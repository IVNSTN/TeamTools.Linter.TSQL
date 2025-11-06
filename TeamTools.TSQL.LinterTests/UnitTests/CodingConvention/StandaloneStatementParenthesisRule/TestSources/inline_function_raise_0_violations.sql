CREATE FUNCTION dbo.foo (@arg INT)
RETURNS TABLE
AS
RETURN (
    SELECT a, b, c FROM bdo.bar
);
