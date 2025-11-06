SET @a = CASE @b
             WHEN 1 THEN
                 'a'
             WHEN 2 THEN
                 'b'
             ELSE
                 CASE /* comment */ (@B) -- same expression
                     WHEN 3 THEN
                         'c'
                 END
         END;
