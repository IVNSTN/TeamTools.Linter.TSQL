DELETE bt
OUTPUT
    deleted.NumEDocument AS ts_no_int
    , deleted.acc_code
    , @err_num_temporary_unavailable AS error_num
    , @error_msg AS error_msg
INTO @results (ts_no_int, acc_code, error_num, error_msg)
FROM
(SELECT * FROM dbo.tbl) AS bt;

UPDATE bt SET dt = GETDATE()
FROM @batch AS bt;
