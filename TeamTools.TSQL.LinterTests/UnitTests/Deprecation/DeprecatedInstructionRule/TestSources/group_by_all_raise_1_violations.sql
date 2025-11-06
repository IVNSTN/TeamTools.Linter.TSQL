SELECT TOP 10
       treaty
       , COUNT(*)
FROM dbo.treaty_list AS ta
WHERE treaty BETWEEN 5000 AND 6000
GROUP BY ALL ta.treaty;
