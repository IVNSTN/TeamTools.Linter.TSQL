CREATE PROC dbo.[новая хранимка]
AS
BEGIN
    DECLARE
        @var          INT
        , @переменная VARCHAR(10);

    CREATE TABLE [схема].таблица (
        колонка_в2 INT
    ) 

    INSERT схема.таблица (колонка) 
    OUTPUT INSERTED.* INTO dbo.out_table 
    VALUES (@переменная);
END;
GO
CREATE TABLE #Test (ID int)
DECLARE @table TABLE (name VARCHAR(10))

CREATE SERVICE MainEntities_TargetService
    AUTHORIZATION dbo
    ON QUEUE dbo._TargetQueue_MainEntities
    ([//my_company/SQL/backend_MainEntities_contract], [//my_company/SQL/backend_MainEntities_contract_a76t]);
GO
