CREATE PROC dbo.[новая_хранимка]
AS
BEGIN
    DECLARE
        @var          INT
        , @переменная VARCHAR(10);

    CREATE TABLE [схема].[#_123xx] (
        [vint] INT
    ) 

    INSERT схема.таблица (колонка) 
    OUTPUT INSERTED.* INTO dbo.[out_RETURN] 
    VALUES (@переменная);
END;
