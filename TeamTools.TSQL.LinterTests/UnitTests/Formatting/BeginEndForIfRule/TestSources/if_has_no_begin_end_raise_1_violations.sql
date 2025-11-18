IF 1=0
    SELECT case when 'a' = 'b' THEN 0 ELSE 1 END as test
ELSE
BEGIN
    RETURN;
END
