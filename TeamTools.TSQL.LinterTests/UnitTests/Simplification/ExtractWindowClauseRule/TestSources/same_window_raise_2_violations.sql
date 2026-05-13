SELECT SalesOrderID,
    ProductID,
    OrderQty,
    SUM(OrderQty) OVER (PARTITION BY SalesOrderID ORDER BY OrderQty) AS [Total],
    -- ASC is the default sort order
    AVG(OrderQty) OVER (PARTITION BY SalesOrderID ORDER BY OrderQty ASC) AS [Avg],              -- 1
    -- RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW is the default range definition
    COUNT(OrderQty) OVER (PARTITION BY [SalesOrderID] ORDER BY OrderQty
        RANGE BETWEEN UNBOUNDED PRECEDING AND CURRENT ROW) AS [Count]     -- 2
FROM Sales.SalesOrderDetail
WHERE SalesOrderID IN (43659, 43664);
