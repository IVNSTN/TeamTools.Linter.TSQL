DECLARE @errortext VARCHAR(100), @value int;

RAISERROR (@errortext, 16, @value);
RAISERROR ('err', 16, @value);
RAISERROR (@value, 16, @value);
RAISERROR (@value, 16, 0);
