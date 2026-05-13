-- no WINDOW clause
SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER (PARTITION BY SalesOrderID) AS [Total]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664);

-- name "foo" resolved
SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER foo AS [Total]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664)
WINDOW foo AS (PARTITION BY SalesOrderID);
