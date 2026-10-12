CREATE TABLE dbo.customer
(
    id INT NOT NULL,
    name NVARCHAR(100) NOT NULL
)
GO

CREATE TABLE dbo.invoice
(
    invoice_id INT NOT NULL,
    customer_id INT NOT NULL,
    CONSTRAINT PK_invoice PRIMARY KEY (invoice_id),
    CONSTRAINT UQ_invoice_id UNIQUE (invoice_id),
    CONSTRAINT CK_invoice_amount CHECK (invoice_id > 0)
)
GO
