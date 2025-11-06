DECLARE @str VARCHAR(10);

SELECT 1
WHERE 1=1 and (1=0 OR
    LOWER('asdf', 'asdf') IS NULL OR
    UPPER('asdf', 'asdf') IS NULL OR
    LTRIM('asdf', 'asdf') IS NULL OR
    RTRIM('asdf', 'asdf') IS NULL OR
    LEFT(@str, 2, @str) IS NULL OR
    RIGHT(@str, 1, @str) IS NULL OR
    ISJSON(@str, @str) IS NULL OR
    REPLACE(@str, 'asdf', 'zx', @str) IS NULL OR
    ISNULL(@str, 'dsaf', @str) IS NULL OR
    --NULLIF(@str, '', @str) IS NULL OR
    DATEADD(dd, '1990101', 3, 3) IS NULL OR
    DATEDIFF(dd, '1990101', GETDATE(), 4) IS NULL OR
    DATENAME(DAY, GETDATE(), 5) IS NULL OR
    DATEFROMPARTS(2000, 10, 20, 6) IS NULL OR
    YEAR('20210301', @str) IS NULL OR
    DAY('20210301', @str) IS NULL OR
    MONTH('20210301', @str) IS NULL OR
    SIGN(1, a) IS NULL OR
    COUNT(1, 2) IS NULL OR
    AVG(1, @str) IS NULL OR
    SUM(1, @str) IS NULL OR
    GETDATE(NULL) IS NULL OR
    SYSDATETIME('asfd') IS NULL OR
    NEWID(0) IS NULL)
