DECLARE
    @foo INT,
    @orders CURSOR

SET @orders = CURSOR FAST_FORWARD FOR
SELECT id
FROM dbo.orders
