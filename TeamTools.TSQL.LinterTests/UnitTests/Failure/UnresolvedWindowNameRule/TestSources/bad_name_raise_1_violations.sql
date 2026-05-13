SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER foo AS [Total]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664)
WINDOW bar AS (PARTITION BY SalesOrderID);
