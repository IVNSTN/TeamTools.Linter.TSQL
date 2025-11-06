-- this test is mostly for avoiding dup violations
IF @a = @b
BEGIN
    SELECT @a;
END -- 1

WHILE @a < @b
BEGIN
    SET @a += 1;
END -- 2
