DECLARE @a CHAR(1)
SELECT
    CASE @a
    WHEN
        '3'
    -- comment
    THEN 3
    WHEN
        '4'
        -- comment
    THEN
        4
    ELSE
    /*

    coment
    */
    5
END
