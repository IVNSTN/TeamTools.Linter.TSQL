CREATE TABLE #tmp
(
    id      INT  NOT NULL,
    a_date  DATE NOT NULL
) ON DatePartitioning(a_date);

ALTER TABLE #tmp SET (LOCK_ESCALATION = AUTO);
