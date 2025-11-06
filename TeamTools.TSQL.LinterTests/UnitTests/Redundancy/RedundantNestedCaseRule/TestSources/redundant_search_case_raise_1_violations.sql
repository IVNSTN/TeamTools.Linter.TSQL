SET @a = CASE
             WHEN @b = 1 THEN
                 'a'
             WHEN @b = 2 THEN
                 'b'
             ELSE -- comment
                 CASE
                     WHEN @c = 0 THEN
                         'c' -- collapse with outer case
                 END
         END;
