DECLARE @var VARCHAR(100) = dbo.unknown_fn()

SELECT
    TRIM(@var, ' '), -- space is the default char to trim
    OBJECT_NAME(123, DB_ID()), -- current db is the default context
    CONCAT(@var, 'asdf', '', NULL) -- null and empty string make no effect
