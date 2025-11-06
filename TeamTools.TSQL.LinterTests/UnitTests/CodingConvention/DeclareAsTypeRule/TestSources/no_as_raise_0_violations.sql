CREATE PROC dbo.foo
    @param INT
    , @t   my.tbl_type READONLY
AS
BEGIN
    DECLARE
        @var  dbo.my_type
        , @f  FLOAT
        , @cr CURSOR;
END;
GO

DECLARE
    @var  dbo.my_type
    , @f  FLOAT
    , @cr CURSOR;
