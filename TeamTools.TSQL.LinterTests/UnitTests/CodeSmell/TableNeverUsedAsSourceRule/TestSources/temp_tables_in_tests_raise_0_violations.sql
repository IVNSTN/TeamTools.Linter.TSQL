CREATE TABLE #Expected(name VARCHAR(100))
CREATE TABLE #Actual(name VARCHAR(100))

EXEC tSQLt.AssertEmptyTable '#Expected';

EXEC tSQLt.AssertEqualsTable
    @Expected = @expected
    , @Actual = N'#Actual'
