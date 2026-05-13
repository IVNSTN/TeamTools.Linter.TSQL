SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER (PARTITION BY SalesOrderID) AS [Total]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664)
WINDOW win AS (PARTITION BY SalesOrderID);      -- 1

SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER foo AS [Total]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664)
WINDOW
    foo AS (PARTITION BY SalesOrderID ORDER BY SalesOrderID),
    bar AS (PARTITION BY ProductID);      -- 2
