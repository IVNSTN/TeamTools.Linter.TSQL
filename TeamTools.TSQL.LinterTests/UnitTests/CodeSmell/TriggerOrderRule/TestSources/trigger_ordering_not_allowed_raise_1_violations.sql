EXECUTE sp_settriggerorder @triggername = N'[mrgn].[TR_ActPosShort_IU_repl]', @order = N'last', @stmttype = N'insert';
declare @proc sysname
EXEC @proc @triggername = N'[mrgn].[TR_ActPosShort_IU_repl]', @order = N'last', @stmttype = N'insert';
