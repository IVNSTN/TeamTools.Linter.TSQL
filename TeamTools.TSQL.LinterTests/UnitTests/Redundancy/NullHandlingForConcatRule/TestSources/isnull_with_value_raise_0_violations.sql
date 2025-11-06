SELECT CONCAT('asdf', @var, ISNULL(@bar, (('asdf'))))

SET @bar = CONCAT_WS('-', COALESCE(@far, 'vcxz'))
