-- compatibility level min: 130
CREATE PROCEDURE bar
AS
BEGIN ATOMIC WITH (TRANSACTION ISOLATION LEVEL = SNAPSHOT)
    -- cannot be reset in atomic
    SELECT 1
END;
