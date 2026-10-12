-- compatibility level max: 100
SELECT region, product, amount
FROM sales
COMPUTE SUM (amount) BY region
