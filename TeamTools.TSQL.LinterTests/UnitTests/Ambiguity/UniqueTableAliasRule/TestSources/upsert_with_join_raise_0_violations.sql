CREATE PROCEDURE dbo.foo
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM del
    OUTPUT deleted.record_id
    INTO @deleted_records (record_id)
    FROM dbo.bar AS del
    INNER JOIN @batch AS bt
        ON bt.record_id = del.record_id
    WHERE bt.act_type = 'D';

    UPDATE upd
    SET
        placement_type = bt.placement_type
    FROM dbo.bar as upd
    INNER JOIN @batch AS bt
        ON bt.record_id = upd.record_id
    WHERE bt.act_type <> 'D';

    INSERT INTO dbo.bar
    (
        record_id
    )
    SELECT
        bt.record_id
    FROM @batch AS bt
    LEFT JOIN dbo.bar AS ins
        ON bt.record_id = ins.record_id
    WHERE ins.record_id IS NULL
        AND bt.act_type <> 'D';
END;
GO
