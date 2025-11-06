DECLARE @action CHAR(1)

UPDATE ord
SET
    title = NULL,
    @action = CASE
                    WHEN 1=0 THEN
                        'W'
                    WHEN 2=1 THEN
                        'E'
                    ELSE
                        'D'
                END
FROM dbo.orders ord
WHERE ord.id = @id

SELECT @action
