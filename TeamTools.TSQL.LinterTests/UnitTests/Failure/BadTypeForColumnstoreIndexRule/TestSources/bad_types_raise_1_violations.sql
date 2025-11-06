CREATE TABLE arch.Orders
(
    dt             DATETIME   NOT NULL
    , SignType     INT        NULL
    , OrderChannel ROWVERSION NULL -- bad
    , Flags        XML        NULL -- bad
);
GO

CREATE CLUSTERED COLUMNSTORE INDEX clcs_ArchOrders
    ON arch.Orders
    WITH (DATA_COMPRESSION = COLUMNSTORE_ARCHIVE)
    ON [Dt2RangePS](dt);
GO
