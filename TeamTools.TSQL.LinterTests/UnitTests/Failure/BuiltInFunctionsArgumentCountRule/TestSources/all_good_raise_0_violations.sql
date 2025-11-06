DECLARE @str VARCHAR(10);

SELECT
    LOWER('asdf')
    , UPPER('asdf')
    , LTRIM('asdf')
    , RTRIM('asdf')
    , LEFT(@str, 2)
    , RIGHT(@str, 1)
    , CAST(@str AS INT)
    , ISJSON(@str)
    , REPLACE(@str, 'asdf', 'zx')
    , ISNULL(@str, 'dsaf')
    , NULLIF(@str, '')
    , DATEADD(dd, 3, '19900101')
    , DATEDIFF(dd, '19900101', GETDATE())
    , DATENAME(DAY, GETDATE())
    , DATEFROMPARTS(2000, 10, 20)
    , YEAR('20210301')
    , DAY('20210301')
    , MONTH('20210301')
    , SIGN(1)
    , COUNT(1)
    , AVG(1)
    , SUM(1)
    , GETDATE()
    , SYSDATETIME()
    , NEWID()
    -- parameterless calls
    , CURRENT_TIMESTAMP
    , SYSTEM_USER
    -- not handled
    , CONVERT(int, @str, 103)
    , CASE @str WHEN 'a' THEN 0 END
    , ROW_NUMBER() OVER ( ORDER BY (SELECT @str)) as rn;
