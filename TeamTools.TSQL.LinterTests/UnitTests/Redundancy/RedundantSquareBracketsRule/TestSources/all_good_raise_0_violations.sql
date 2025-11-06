CREATE PROC dbo.[новая хранимка]
AS
BEGIN
    DECLARE
        @var          INT
        , @переменная VARCHAR(10);

    CREATE TABLE [схема ].[@ 123 xx ] (
        [int] INT
    ) 

    SELECT foo.bar as [@bar]
    FROM foo
    FOR XML AUTO

    INSERT схема.таблица (колонка) 
    OUTPUT INSERTED.* INTO dbo.[RETURN] 
    VALUES (@переменная);
END;
GO

GRANT EXEC ON OBJECT::dbo.[новая хранимка]
TO PUBLIC
GO

DENY SELECT ON OBJECT::схема.таблица
TO PUBLIC
GO
