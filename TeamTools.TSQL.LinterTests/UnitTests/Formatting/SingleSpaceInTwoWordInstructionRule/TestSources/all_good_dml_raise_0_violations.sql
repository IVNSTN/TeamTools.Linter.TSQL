begin try
    BEGIN TRAN

    SELECT *
    from foo as bar
    INNER JOIN tbl
        ON tbl.id = bar.id
    OUTER APPLY (select 1 from adsf) t
    group by col
    order by name

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

begin TRY
    SELECT 1
end try
begin catch
end catch

select row_number() over(PARTITION BY id ORDER by name ) rn
FROM tbl
