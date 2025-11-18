DELETE dbo.foo
WHERE parent_id IS NULL

SET @r = @@ROWCOUNT

IF @r > 0
BEGIN
    -- actually such scenarios are now ignored by the rule
    -- complex nested predicate dups should be detected by "always true/false predicate" detection rule
    SET @r = @r - 10

    IF @r > 0   -- it is not the same anymore
    BEGIN
        SELECT @r = r FROM dbo.bar WHERE id = 100

        IF @r > 0   -- it is not the same anymore
        BEGIN
            RETURN
        END
    END
END
