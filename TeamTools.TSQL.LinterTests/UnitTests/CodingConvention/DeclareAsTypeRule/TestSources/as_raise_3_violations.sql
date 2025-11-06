CREATE PROC dbo.foo
    @param AS INT                   -- 1
    , @t   my.tbl_type READONLY
AS
BEGIN
    DECLARE
        @var  AS dbo.my_type        -- 2
        , @f  FLOAT
        , @cr AS CURSOR;            -- 3
END;
GO
