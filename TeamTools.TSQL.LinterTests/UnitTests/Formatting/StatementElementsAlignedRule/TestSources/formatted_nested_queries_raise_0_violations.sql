-- compatibility level min: 110
SELECT TOP (1)
        CAST(IIF(
                    EXISTS
                    (
                        SELECT 0
                        FROM dbo.bar AS aa
                        INNER JOIN dbo.far AS sa
                            ON sa.acc_code = aa.acc_code
                        INNER JOIN dbo.jar AS acc
                            ON acc.treaty = sa.treaty
                        WHERE aa.sys_name = @sys_name
                            AND aa.access_level > 0
                    )
                    , 1
                    , 0) AS BIT) AS some_flag
        , RTRIM(man.full_name) AS manager_full_name
        , cli.pin
FROM dbo.managers AS man
INNER JOIN dbo.employees AS ep
    ON ep.emp_id = man.emp_id
LEFT JOIN dbo.clients AS cli
    ON cli.cl_id = man.cl_id
WHERE man.sys_name = @sys_name
    AND NOT EXISTS (
                        SELECT 0
                        FROM (
                            SELECT TOP 1 *
                            FROM dbo.zar AS aa
                            INNER JOIN dbo.car AS sa
                                ON sa.dar = aa.car
                            WHERE aa.tar = @sys_name
                            ORDER BY 1
                        ) sa
                        INNER JOIN dbo.accounts AS acc
                            ON acc.treaty = sa.treaty);
