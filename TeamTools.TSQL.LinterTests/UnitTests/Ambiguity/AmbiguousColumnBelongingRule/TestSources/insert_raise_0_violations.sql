INSERT INTO #PriceRaw (DatePrice, ActId, PriceCurrencyId, Price, SourceId, DateSource, Priority)
SELECT
    DatePrice
    , act_id AS ActId
    , PriceCurrencyId
    , Price
    , SourceId
    , DATEADD(DD, 1, DateSource) AS DateSource
    , Priority
FROM cte
WHERE (Price > 0 AND ISNULL(DateSource, @EmptyDate) BETWEEN @DateFrom AND @DateTo);
