BEGIN TRAN my_tran      -- 1 never closed
SAVE TRAN my_savepoint  -- 2 never closed
ROLLBACK TRAN svpnt     -- 3 never opened
COMMIT TRAN mtrn        -- 4 never opened
