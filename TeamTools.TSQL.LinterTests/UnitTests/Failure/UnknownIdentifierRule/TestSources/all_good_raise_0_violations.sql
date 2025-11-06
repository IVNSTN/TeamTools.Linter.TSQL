DECLARE
    @A    SYSNAME
    , @dt DATETIME2(0) = SYSDATETIME();

EXEC @A @b = 'zxcv';

IF @@ROWCOUNT > 0
    SELECT SCOPE_IDENTITY();

SELECT x AS [@asdf] FROM OPEN_QUERY() AS o CROSS JOIN dbo.foo AS f WHERE ISJSON('asdf') = 1;

CREATE INDEX ix_zar
    ON foo.bar (val)
    ON dt2fs(a_date);

UPDATE x
SET a = DATEADD(DD, GETDATE(), 1)
OUTPUT inserted.a
INTO @acme (name)
FROM dbo.xxx
WHERE c = 'd';

RECEIVE TOP (1)
    conversation_handle
    , CONVERT(VARBINARY(MAX), message_body)
    , message_type_name
    FROM Balances._TargetQueue
    WHERE CONVERSATION_GROUP_ID = 'JAR';

RETURN;
