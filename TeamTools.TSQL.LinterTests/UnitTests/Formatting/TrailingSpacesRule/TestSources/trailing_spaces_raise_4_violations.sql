DECLARE
    -- below - 1
    @a INT   
    ,  @b INT
    -- below - 2
    , @c DECIMAL(18,   2);  
    -- below - 3
SET @a = CASE WHEN 1=2
    THEN    @b
    ELSE @c    END;
  
SELECT 'a',

-- below - 4
'b'    
