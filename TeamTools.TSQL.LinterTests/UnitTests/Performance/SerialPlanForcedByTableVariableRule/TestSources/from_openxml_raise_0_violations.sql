INSERT @Customers(cust_id, contact_name)
SELECT *
FROM OPENXML(@idoc, '/ROOT/Customer', 1)
WITH (
    CustomerID  VARCHAR(10),
    ContactName VARCHAR(20)
);
