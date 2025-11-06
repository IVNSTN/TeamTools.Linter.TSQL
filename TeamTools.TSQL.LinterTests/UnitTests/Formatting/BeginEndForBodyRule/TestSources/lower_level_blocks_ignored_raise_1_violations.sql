-- compatibility level min: 110
CREATE PROCEDURE dbo.foo
AS
    DECLARE @a INT

    IF @a = 1
    BEGIN
        SELECT 'x';
    END

    begin try
        select 1
    end try
    begin catch
        throw
    end catch
