DECLARE @txt VARCHAR(10) = 'abcde'

SELECT
    @txt = REPLACE(@txt, 'a', 'x')
    , @txt = REPLACE(@txt, 'b', 'x')
    , @txt = REPLACE(@txt, 'c', 'x')
    , @txt = REPLACE(@txt, 'd', 'x')
    , @txt = REPLACE(@txt, 'e', 'x')

PRINT @txt
