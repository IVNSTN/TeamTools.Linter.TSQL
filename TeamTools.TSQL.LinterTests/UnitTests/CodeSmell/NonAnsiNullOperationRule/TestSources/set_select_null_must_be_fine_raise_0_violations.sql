   select dateadd( d, row_number() over(order by (select null)) - 1, '20100101') dt from e

   SELECT ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) / 8 AS groupID

   IF (SELECT trade FROM f_get_is_trade(@today_d, NULL) ) = 'Нет'
        SET @a = (NULL)
