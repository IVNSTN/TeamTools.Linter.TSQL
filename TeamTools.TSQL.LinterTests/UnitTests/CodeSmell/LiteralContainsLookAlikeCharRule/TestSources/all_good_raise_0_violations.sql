select '', NULL, 'adsf'             -- latin only

declare @str VARCHAR(100) = 'АБВГД' -- cyrillic only

exec dbo.foo
    @arg = 'qwerty zxcv 123'        -- lating + numbers


-- valid mix of c/с, A/А
print 'Переменной @c присвоено неверное значение ''A''!'
