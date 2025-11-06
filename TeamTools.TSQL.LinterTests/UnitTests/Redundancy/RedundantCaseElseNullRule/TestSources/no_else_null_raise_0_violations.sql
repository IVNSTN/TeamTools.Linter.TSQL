SELECT
    CASE @foo
        WHEN 'a' THEN 1
        WHEN 'b' THEN 2
        ELSE 0
      END
    , CASE
          WHEN @bar = 0 THEN
              'a'
          WHEN @bar = 1 THEN
              'b'
      -- no ELSE
      END;
