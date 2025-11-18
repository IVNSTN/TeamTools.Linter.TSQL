IF @val IN (@a, 'asdf', ((SELECT @a)))
    PRINT '1'
