SELECT 1 from [cursor]

BEGIN select a 
    END -- 1
;

IF (1=0)
BEGIN
    SELECT a

    BEGIN
        RETURN 0;
END; END; -- 2, 3
