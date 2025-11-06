CREATE PROC dbo.[новая test хранимка] -- 1
AS
BEGIN
    DECLARE
        @var_число    INT -- 2
        , @переменная VARCHAR(10);

    CREATE TABLE [new схема].таблица (
        колонка_v2 INT -- 3
    ) 

    INSERT [new схема].таблица (колонка_v2) 
    OUTPUT INSERTED.* INTO dbo.out_table 
    VALUES (@переменная);
END;
