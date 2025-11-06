CREATE FUNCTION txt.regex_matches (@input NVARCHAR(MAX), @pattern NVARCHAR(MAX))
RETURNS TABLE (item NVARCHAR(4000) NULL, item_index INT NULL, item_length INT NULL)
AS
    EXTERNAL NAME UtilCLR.RegexRoutines.RegexMatches;
GO
