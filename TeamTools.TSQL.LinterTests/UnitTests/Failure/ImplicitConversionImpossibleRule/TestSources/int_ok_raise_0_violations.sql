DECLARE
    @int INT
    , @smallint SMALLINT
    , @tinyint TINYINT
    , @bit BIT

SET @int = @smallint + @tinyint - @bit

SET @int = ISNULL(@smallint, @tinyint) + 100
