CREATE TABLE #FinInfoExtTable2
(
    IdFI                  INT NOT NULL
    , IdTradePeriodStatus INT NULL
    , IdAllowOrderStatus  INT NULL
    , IdSession           INT NOT NULL
);

INSERT INTO #FinInfoExtTable2
EXEC dbo.ShredXml
    @InputXml = @x
    , @AttributeElementHandling = 0
    , @RootElementName = 'r';
