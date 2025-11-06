SET @bit = IIF(CASE @a WHEN 'a' THEN 1 ELSE 0 END = 1, 1, 0);
