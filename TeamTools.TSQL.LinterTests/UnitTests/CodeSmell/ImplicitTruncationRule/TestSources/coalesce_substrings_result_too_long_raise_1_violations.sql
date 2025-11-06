-- compatibility level min: 110
DECLARE
    @Email       VARCHAR(128)
    , @UserLogin VARCHAR(10)
    , @sys_name  VARCHAR(5)

EXEC dbo.get_email
    @Email = @Email OUTPUT; -- to make it UNKNOWN, not NULL

-- should evaluate right side to max(10, 128-1, 5, 0) = 127
SET @UserLogin = COALESCE(
    @UserLogin,
    IIF(
        CHARINDEX('@', @Email) > 0,
        LEFT(@Email,
            CHARINDEX('@', @Email) - 1),
        @sys_name
       ),
    '');
