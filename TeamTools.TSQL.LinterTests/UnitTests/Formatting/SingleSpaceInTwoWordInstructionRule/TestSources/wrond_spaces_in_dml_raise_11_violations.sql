begin   try         -- 1
    BEGIN   TRAN    -- 2

    SELECT *
    from foo as bar
    INNER   JOIN tbl                    -- 3
        ON tbl.id = bar.id
    OUTER  APPLY (select 1 from adsf) t -- 4
    group    by col
    order                               -- 5
    by name                             -- 6

    commit    tran                      -- 7
end try
begin   catch                           -- 8
    ROLLBACK  TRAN                      -- 9
end   catch                             -- 10

select row_number() over(PARTITION   BY id ORDER by name ) rn -- 11
FROM tbl
