SELECT FORMATMESSAGE('%s %i %s', 'asfd', 1, NULL)   -- count matches
SELECT FORMATMESSAGE(@fmt, 'asfd', 1, NULL)         -- var ignored
SELECT FORMATMESSAGE(50000, 'asfd', 1, NULL)        -- numbered ignored

RAISERROR('%s %s', 16, 1, 'asfd', 'afds'); -- count matches
RAISERROR(@err_msg, 16, 1);   -- error var unknown content
RAISERROR(50000, 16, 1, 'a'); -- error number
RAISERROR('%%s %%i', 16, 1);  -- escaped
