CREATE PROCEDURE dbo.foo
    @date      [CHAR](1)        -- 1
WITH EXECUTE AS CALLER
AS
BEGIN
    DECLARE
        @id      [INT] = 1      -- 2
        , @spot  [GEOGRAPHY]    -- 3
        , @data  [XML]          -- 4
END
