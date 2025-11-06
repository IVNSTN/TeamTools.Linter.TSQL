SELECT Customers.CustID, Customers.Name, Orders.OrderID, Orders.Status  
FROM {oj Customers LEFT OUTER JOIN Orders ON Customers.CustID=Orders.CustID}  
WHERE Orders.Status='OPEN'  
