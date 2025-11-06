CREATE TABLE dbo.acme
(
    id      INT  NOT NULL,
    a_date  DATE NOT NULL
);

ALTER TABLE dbo.acme SET (LOCK_ESCALATION = DISABLE);
