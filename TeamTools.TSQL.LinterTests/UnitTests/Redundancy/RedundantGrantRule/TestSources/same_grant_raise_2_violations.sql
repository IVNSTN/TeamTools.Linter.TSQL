GRANT REFERENCES ON OBJECT::dbo.test
TO usr1;
GO

GRANT REFERENCES ON OBJECT::dbo.test -- 1
TO usr1;
GO

GRANT SEND
    ON SERVICE::[my/fine/brand/svc]
    TO PUBLIC;
GO

GRANT SEND
    ON SERVICE::[my/fine/brand/svc] -- 2
    TO PUBLIC;
GO
