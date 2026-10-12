DECLARE
    @raw_data   VARBINARY(MAX)
    , @l        INT

RECEIVE TOP (1)
    @raw_data = CONVERT(VARBINARY(MAX), message_body)
FROM my.queue;

SELECT @l = CONVERT(TINYINT, SUBSTRING(@raw_data, 127, 1))

SELECT SUBSTRING(@raw_data, 34, @l)
