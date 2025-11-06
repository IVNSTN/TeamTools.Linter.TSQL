SELECT 1 from [cursor]

BEGIN select a END;

IF (1=0)
BEGIN
    SELECT a

    BEGIN
        RETURN 0;
    END
END ;
