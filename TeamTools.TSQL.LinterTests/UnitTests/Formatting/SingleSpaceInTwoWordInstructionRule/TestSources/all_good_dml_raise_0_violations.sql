BEGIN TRY
    BEGIN TRAN

    SELECT *
    FROM foo AS bar
    INNER HASH JOIN tbl
        ON tbl.id = bar.id
    OUTER APPLY (SELECT 1 FROM adsf) t
    GROUP BY col
    ORDER BY name

    select *
    FROM inserted AS i
    CROSS APPLY
    (
        SELECT i.current_course AS cc
        WHERE i.money_act_id <> i.price_money_act_id
            OR i.pay_currency NOT IN ('USD', 'EUR', 'JPY')
    ) AS ic
    INNER JOIN dbo.actives AS act
        on a = b

    commit tran
end try
begin catch
    ROLLBACK TRAN
end catch

COMMIT TRANSACTION;

BEGIN TRY
    BEGIN
        SELECT 1
    END
END TRY
BEGIN CATCH
    ROLLBACK
END CATCH

select row_number() over(PARTITION BY id ORDER by name ) rn
FROM tbl
GO

TRUNCATE TABLE dbo.foo

INSERT INTO dbo.foo (id)
VALUES (1)

INSERT dbo.foo (id)
VALUES (1)

DELETE dbo.foo WHERE 1 = 0

DELETE FROM t
FROM dbo.tbl AS t
WHERE 1 = 0

DELETE t
FROM dbo.tbl AS t
WHERE 1 = 0

DELETE TOP(10)
FROM dbo.tbl
WHERE 1 = 0
