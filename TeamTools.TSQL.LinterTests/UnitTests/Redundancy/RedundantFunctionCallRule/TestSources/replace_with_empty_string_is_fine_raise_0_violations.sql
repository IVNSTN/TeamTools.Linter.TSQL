SET @var = REPLACE('asdf', 'a', '');
SET @var = STUFF('asdf', 1, 2, '');
SET @var = ISNULL(@other_var, '');
SET @var = NULLIF(@other_var, '');
SET @var = COALESCE(@other_var, @var_3, '');
