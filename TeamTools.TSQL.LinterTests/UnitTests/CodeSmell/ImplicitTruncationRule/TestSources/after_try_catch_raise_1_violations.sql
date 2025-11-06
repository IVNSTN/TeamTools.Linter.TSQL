DECLARE
    @src   NCHAR(20)
    , @dst NCHAR(3);

BEGIN TRY
    SET @src = 'A';
END TRY
BEGIN CATCH
    SET @src = 'BCDEFG'; -- longer than 3
END CATCH;

-- We don't know either TRY block succeeded or we fell into CATCH block
-- so the actual value of @src is considered unpredictable
-- thus implicit truncation from 6 to 3 symbols should be issued.
SET @dst = @src;
