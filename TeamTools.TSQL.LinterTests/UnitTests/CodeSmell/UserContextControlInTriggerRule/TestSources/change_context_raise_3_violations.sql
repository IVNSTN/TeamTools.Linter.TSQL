CREATE TRIGGER trg on dbo.orders 
WITH EXECUTE AS 'asdf'
AFTER UPDATE
AS
BEGIN
    EXECUTE AS login = 'adsf';
    SELECT 1;
    REVERT;
END
