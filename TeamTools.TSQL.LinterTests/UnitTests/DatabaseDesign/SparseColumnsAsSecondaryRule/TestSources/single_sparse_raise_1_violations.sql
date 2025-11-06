CREATE TABLE dbo.bar
(
    id  INT  NOT NULL,
    dt  DATE NOT NULL DEFAULT GETDATE(),
    bar INT NULL,
    far BIT SPARSE NULL, -- here
    jar CHAR(1) NULL
)
GO
