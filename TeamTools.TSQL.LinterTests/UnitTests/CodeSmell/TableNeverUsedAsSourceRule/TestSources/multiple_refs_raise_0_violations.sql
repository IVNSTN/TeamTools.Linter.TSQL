CREATE PROC dbo.test
AS
BEGIN
    CREATE TABLE #Expected
    (
        id          INT           NOT NULL
        , val_type  VARCHAR(2)    NOT NULL
    );

    INSERT #Expected (id, val_type)
    VALUES
    (1, 'foo');

    SET @test_data =
    (
        SELECT
            'I' AS ActType
            , ee.id AS ValueId
            , ee.val_type AS ValueType
        FROM #Expected AS ee
        FOR XML AUTO
    );

    EXEC tSQLt.AssertEqualsTable
        @Expected = N'#Expected'
        , @Actual = N'#Actual';
END
GO
