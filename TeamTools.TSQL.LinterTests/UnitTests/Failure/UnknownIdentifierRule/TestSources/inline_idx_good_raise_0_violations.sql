-- compatibility level min: 130
DECLARE @acme TABLE
(
    id INT,
    name dbo.some_type DEFAULT ('x') NOT NULL,
    INDEX ix_test (id, name)
);
