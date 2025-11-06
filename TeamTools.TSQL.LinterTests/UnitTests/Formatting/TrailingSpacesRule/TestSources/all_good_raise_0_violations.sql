DECLARE
    @a INT
    , @b    INT
    , @c DECIMAL(18, 2);

SET @a = CASE WHEN 1=2
    THEN @b
    ELSE @c
    END;

select 'a' ,
'b'
