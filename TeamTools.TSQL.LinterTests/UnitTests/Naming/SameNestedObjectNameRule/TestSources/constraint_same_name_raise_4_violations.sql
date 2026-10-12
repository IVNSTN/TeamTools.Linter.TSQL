CREATE TABLE dbo.account
(
    id INT NOT NULL,
    CONSTRAINT account PRIMARY KEY CLUSTERED (id),
    CONSTRAINT account CHECK (id > 0),
    CONSTRAINT account UNIQUE (id),
    CONSTRAINT account FOREIGN KEY (id) REFERENCES master(id)
)
GO
