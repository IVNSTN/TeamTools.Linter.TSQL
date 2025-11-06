declare @errortext VARCHAR(100), @value int;

RAISERROR (@errortext, 0, 1);
RAISERROR ('err', 1, 1);
RAISERROR (50000, 3, 1);
