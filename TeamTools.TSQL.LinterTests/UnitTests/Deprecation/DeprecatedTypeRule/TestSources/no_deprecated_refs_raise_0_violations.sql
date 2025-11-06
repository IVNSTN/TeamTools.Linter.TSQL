DECLARE 
    @timestamp INT, 
    @mytype    sysname;

DECLARE @foo TABLE
(
    id DATE,
    rv ROWVERSION,
    calc as 1
)
