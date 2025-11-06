INSERT INTO #NewBankAccounts (Treaty, PlaceCodeList)
SELECT
mt.Treaty
, STRING_AGG(mt.PlaceCode, ',') AS PlaceCodeList
FROM (SELECT DISTINCT Treaty, PlaceCode FROM #MissingTrades WHERE AccCode IS NULL AND PlaceCode IS NOT NULL) AS mt

SELECT *
FROM
(
    SELECT
        p_code
        , place_code
        , SUM(real_rest) AS real_rest
    FROM dbo.balance
    WHERE (treaty BETWEEN @treaty_min AND @treaty_max)
        AND p_code IN ('USD', 'EUR', 'GBP')
    GROUP BY
        p_code
        , place_code
) AS bb;
