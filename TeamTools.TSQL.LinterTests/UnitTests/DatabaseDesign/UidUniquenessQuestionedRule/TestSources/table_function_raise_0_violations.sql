CREATE FUNCTION dbo.RegexMatches (@input NVARCHAR(4000), @pattern NVARCHAR(4000))
RETURNS TABLE
(
    Item         NVARCHAR(4000) NULL
    , ItemIndex  INT            NULL
    , ItemLength INT            NULL
)
AS
    EXTERNAL NAME UtilCLR.RegexRoutines.RegexMatches;
