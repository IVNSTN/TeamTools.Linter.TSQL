
SELECT 1
FROM Sales.SalesOrderDetail

SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER (PARTITION BY SalesOrderID) AS [Total]
FROM Sales.SalesOrderDetail

SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER win AS [Total]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664)
WINDOW win AS (PARTITION BY SalesOrderID);

SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER foo AS [Total]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664)
WINDOW
    foo AS (bar), -- bar name reused here
    bar AS (PARTITION BY SalesOrderID);
