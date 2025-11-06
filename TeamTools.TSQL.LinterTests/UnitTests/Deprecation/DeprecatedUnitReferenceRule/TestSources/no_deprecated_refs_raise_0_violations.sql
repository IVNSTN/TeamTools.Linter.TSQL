SELECT dbo.f_test('a'), GETDATE()
FROM dbo.foo
WHERE bar = 'far';

DECLARE 
    @war INT, 
    @proc sysname;

EXEC zar.dar
    @jar = 'mar'
    , @nar = @war OUTPUT;
                
exec @proc;
