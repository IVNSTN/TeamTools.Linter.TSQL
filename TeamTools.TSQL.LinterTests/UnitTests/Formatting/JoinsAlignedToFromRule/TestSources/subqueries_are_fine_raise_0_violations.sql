SELECT TOP (1)
        CAST((CASE WHEN
          EXISTS
          (
              SELECT 0
              FROM dbo.op_access AS aa
              INNER JOIN dbo.accounts AS sa
                  ON sa.account_code = aa.account_code
              WHERE aa.user_name = @usr_name
                  AND (acc.status_type IS NULL OR acc.status_type <> 'BLOCKED')
                  AND aa.access_level > 0
          )
          THEN 1
          ELSE 0
          END) AS BIT) AS operation_allowed
        , RTRIM(man.full_name) AS manager_full_name
        , cli.pin
FROM dbo.managers AS man
LEFT JOIN dbo.clients AS cli
    ON cli.client_id = man.client_id
WHERE man.mng_name = @mng_name
    AND NOT EXISTS (
      SELECT 0
      FROM (
          SELECT TOP 1 *
          FROM dbo.op_access AS aa
          INNER JOIN dbo.accounts AS sa
              ON sa.account_code = aa.account_code
          WHERE aa.user_name = @usr_name
          ORDER BY 1
      ) sa
      INNER JOIN dbo.blocked_accounts AS ba
          ON ba.acc_id = sa.acc_id);
