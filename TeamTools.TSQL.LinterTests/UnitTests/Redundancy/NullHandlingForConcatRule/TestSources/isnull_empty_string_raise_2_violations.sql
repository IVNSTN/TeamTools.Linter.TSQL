SELECT CONCAT('asdf', @var, ISNULL(@bar, ((''))))

SET @bar = CONCAT_WS('-', COALESCE(@far, ''))
