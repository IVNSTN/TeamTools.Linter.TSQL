-- different windows
SELECT SalesOrderID,
       ProductID,
       OrderQty,
       SUM(OrderQty) OVER (PARTITION BY SalesOrderID ORDER BY ProducId ASC) AS [Total],
       SUM(OrderQty) OVER (PARTITION BY SalesOrderID ORDER BY ProducId DESC) AS [TotalDesc],
       AVG(OrderQty) OVER (PARTITION BY SalesOrderID) AS [Avg],
       COUNT(OrderQty) OVER (PARTITION BY ProductID ORDER BY SalesOrderID) AS [Count],
       MIN(OrderQty) OVER (ORDER BY ProductID) AS [Min],
       MAX(OrderQty) OVER (PARTITION BY ProductID ROWS BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS [Max],
       MAX(OrderQty) OVER (PARTITION BY ProductID RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS [Max2],
       MAX(OrderQty) OVER (PARTITION BY ProductID ROWS BETWEEN 2 PRECEDING AND 2 FOLLOWING) AS [Max2]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664);

-- window clause
SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER win AS [Total],
    AVG(OrderQty) OVER win AS [Avg],
    COUNT(OrderQty) OVER win AS [Count],
    MIN(OrderQty) OVER win AS [Min],
    MAX(OrderQty) OVER win AS [Max]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664)
WINDOW win AS (PARTITION BY SalesOrderID);


SELECT SalesOrderID,
    ProductID,
    OrderQty,
    COUNT(OrderQty) OVER foo AS [Count]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664)
WINDOW bar AS (PARTITION BY SalesOrderID);

-- different ranges
SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER (ORDER BY SalesOrderID ROWS BETWEEN 3 PRECEDING AND 2 FOLLOWING) AS [Total],
    AVG(OrderQty) OVER (ORDER BY SalesOrderID ROWS BETWEEN 2 PRECEDING AND 3 FOLLOWING) AS [Avg]
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664);
