SELECT @ErrMess =
    OBJECT_SCHEMA_NAME(@@PROCID ) -- 1
    + ISNULL('state = ' + CONVERT(VARCHAR(30), ERROR_STATE()  ), NEWID()) -- 2

SET @Subject = CAST(( @Counter - 1) AS VARCHAR(10 )) -- 3, 4

SELECT COUNT ( * ) FROM bar
