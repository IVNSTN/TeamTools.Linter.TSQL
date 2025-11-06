CREATE TABLE dbo.acme
(
    id      INT  NOT NULL,
    a_date  DATE NOT NULL
) ON DatePartitioning(a_date);
GO

ALTER TABLE dbo.acme SET (LOCK_ESCALATION = DISABLE);
