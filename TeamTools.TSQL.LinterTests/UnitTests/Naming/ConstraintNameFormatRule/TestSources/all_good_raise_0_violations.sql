CREATE TABLE dbo.test (
    id INT NOT NULL,
    a  INT NOT NULL CONSTRAINT DF_test_a DEFAULT (0),
    b  INT NULL
    CONSTRAINT PK_dbo_test PRIMARY KEY (id)
)

ALTER TABLE dbo.test ADD CONSTRAINT CK_dbo_test_allowed_values_for_a CHECK (a > 0);
GO
ALTER TABLE dbo.test ADD CONSTRAINT UQ_test_b_a UNIQUE (b, a);
GO
ALTER TABLE dbo.test ADD CONSTRAINT DF_dbo_test_a DEFAULT 0 FOR a;
