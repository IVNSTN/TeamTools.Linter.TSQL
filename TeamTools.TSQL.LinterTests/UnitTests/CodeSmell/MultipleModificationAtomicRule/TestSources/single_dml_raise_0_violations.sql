INSERT foo (a, b)
VALUES (1, 2)
GO

-- branches are independent
IF 1=1
    INSERT foo (a, b)
    VALUES (1, 2)
ELSE
    INSERT foo (a, b)
    VALUES (1, 2)
