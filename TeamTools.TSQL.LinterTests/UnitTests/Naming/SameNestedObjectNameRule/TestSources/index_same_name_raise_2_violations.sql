-- compatibility level min: 120
CREATE TABLE dbo.product
(
    id INT NOT NULL,
    INDEX product (id),
    INDEX product NONCLUSTERED (id)
)
GO
