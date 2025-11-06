-- no FROM
UPDATE dbo.foo
SET last_mod = GETDATE()
WHERE update_needed = 1;

-- target and source are the same
UPDATE far
SET dt_update = DEFAULT
FROM dbo.far
WHERE update_needed = 1;

UPDATE tbl.my_table
SET dt_update = DEFAULT
FROM tbl.my_table
WHERE update_needed = 1;

UPDATE t
SET dt_update = DEFAULT
FROM tbl.my_table AS t
WHERE update_needed = 1;
