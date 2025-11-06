CREATE TABLE dbo.a_deponet_export
(
    tran_no           dbo.TIndex       NOT NULL
    , db_date         dbo.TDate        NOT NULL
    , CONSTRAINT PK_a_deponet_export PRIMARY KEY CLUSTERED (tran_no ASC, db_date ASC)
);
GO
