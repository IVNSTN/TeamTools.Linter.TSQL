CREATE TABLE dbo.acme
(
    id      INT  NOT NULL,
    a_date  DATE NOT NULL
)
GO

ALTER TABLE dbo.acme SET (LOCK_ESCALATION = AUTO);
