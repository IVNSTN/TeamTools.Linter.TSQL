BEGIN TRAN

RECEIVE TOP (1)
    @a = x
FROM my_queue

IF @@ROWCOUNT > 0
    INSERT t (a) VALUES (@a)

COMMIT TRAN
