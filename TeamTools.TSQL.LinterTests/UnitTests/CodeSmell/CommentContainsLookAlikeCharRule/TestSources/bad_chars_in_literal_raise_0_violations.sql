select '', NULL, 'аdsf' -- unexpected cyrillic 'а' in latin string

declare @str VARCHAR(100) = 'Cтрока' -- unexpected latin 'C' in cyrillic string

exec dbo.foo
    @arg = 'qwerty zxсv 123' -- unexpected cyrillic 'с' in latin string
