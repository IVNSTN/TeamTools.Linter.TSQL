IF 1=0 BEGIN
    SELECT case when 'a' = 'b' THEN 0 ELSE 1 END as test
END ELSE 
BEGIN
    RETURN;
END
