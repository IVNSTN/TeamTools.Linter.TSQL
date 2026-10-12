CREATE TABLE dbo.payment
(
    id INT NOT NULL
)
GO

ALTER TABLE dbo.payment
ADD CONSTRAINT payment UNIQUE (id)
GO

ALTER TABLE dbo.payment
ADD CONSTRAINT payment CHECK (id > 0)
GO
