CREATE PROCEDURE dbo.foo
    @date      [CHAR](1)        -- 1
WITH EXECUTE AS CALLER
AS
BEGIN
    DECLARE
        @id    [INT] = 0        -- 2
END
