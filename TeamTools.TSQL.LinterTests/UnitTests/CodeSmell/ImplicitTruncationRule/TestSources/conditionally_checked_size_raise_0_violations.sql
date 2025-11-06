DECLARE
    @type          VARCHAR(20)
    , @type_values VARCHAR(3);

IF @type IN ('A', 'B', 'C')
BEGIN
    SET @type_values = @type; -- @type cannot be longer 1 chars here
END;

IF @type = 'ABC'
BEGIN
    SET @type_values = @type; -- @type cannot be longer 3 chars here
END;


SET @type_values = CASE
    WHEN (@type) = ('ABC') THEN @type -- @type cannot be longer 3 chars here
    ELSE 'x'
END

SET @type_values = CASE @type
    WHEN 'ABC' THEN @type -- @type cannot be longer 3 chars here
    ELSE 'x'
END

IF LEN(((@type))) < 2
BEGIN
    SET @type_values = @type; -- @type cannot be longer 2 chars here
END;
