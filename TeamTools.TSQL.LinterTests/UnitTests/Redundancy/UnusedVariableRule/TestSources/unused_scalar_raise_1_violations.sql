DECLARE
    @foo   INT         = 1
    , @bar VARCHAR(10) = 'zar';

EXEC dbo.acme @foo = 1, @bar = @bar;
