DELETE dbo.foo
WHERE parent_id IS NULL

IF @@ROWCOUNT = 0
BEGIN
    -- actually such scenarios are now ignored by the rule
    -- complex nested predicate dups should be detected by "always true/false predicate" detection rule
    UPDATE b
    SET state_no = 1
    FROM dbo.bar
    WHERE children_count = 0

    IF @@ROWCOUNT = 0   -- it is not the same anymore
    BEGIN
        INSERT dbo.far
        SELECT id, name
        FROM dbo.jar
        WHERE start_date < GETDATE()
            AND state_no = 2

        IF @@ROWCOUNT = 0   -- it is not the same anymore
        BEGIN
            SET @var = 1

            IF @@ROWCOUNT = 0   -- rowcount here makes no sence but it is not the same anymore
            BEGIN
                RETURN
            END
        END
    END
END
