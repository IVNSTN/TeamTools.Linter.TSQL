-- col value and function results are unpredictable
SELECT f.id
FROM dbo.foo f
WHERE SUBSTRING(f.title, dbo.my_fn(), 1) = 'A'
    AND SUBSTRING(f.title, LEN(title_prefix), 1) = 'A'
    AND LEFT(f.descr, substr_length) = f.some_col
    AND LEFT(f.descr, LEN(descr_length_for_match)) = 'asdf'
