-- table has no name and can lead to failure
CREATE FUNCTION my_fn ()
RETURNS TABLE
AS
    RETURN (SELECT 1, 'adsf');
GO

CREATE FUNCTION dbo.RegexMatches (@input NVARCHAR(4000), @pattern NVARCHAR(4000))
RETURNS TABLE (Item NVARCHAR(4000) NULL, ItemIndex INT NULL, ItemLength INT NULL)
AS
    EXTERNAL NAME UtilCLR.RegexRoutines.RegexMatches;
GO
