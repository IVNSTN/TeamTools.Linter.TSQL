SELECT @z = @b
    , @w = 150
    , @r = GETDATE()
    , @f = dbo.some_func(@f);

SET @b = @b + 2;
SET @f = dbo.some_func(@f);

SELECT @a = (@a + @b);
