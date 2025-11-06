select *
from dbo.tbl as t
inner join #new as old
    on 1=1
inner join #old as new
    on 1=1
cross join same as same
where exists(select 1 from new)

SELECT @rows_to_insert = COUNT(*)
FROM dbo.balance AS bal
LEFT JOIN dbo.archived_balance AS abl
    ON abl.a_date = @curr_date
        AND abl.account_code = bal.account_code
        AND abl.place_code = bal.place_code
WHERE NOT (
                bal.income_rest = 0
                AND bal.real_rest = 0
                AND bal.sum_turnover = 0
                AND bal.turnover = 0
            )
    AND abl.flag IS NULL;
