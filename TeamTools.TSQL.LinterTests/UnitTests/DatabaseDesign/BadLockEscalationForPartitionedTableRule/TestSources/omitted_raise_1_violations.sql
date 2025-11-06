CREATE TABLE dbo.acme
(
    id      INT  NOT NULL,
    a_date  DATE NOT NULL
) ON DatePartitioning(a_date);
-- default LOCK_ESCALATION is TABLE
