CREATE TABLE arch.Orders
(
    dt             DATETIME   NOT NULL
    , SignType     INT        NULL
    , OrderChannel ROWVERSION NULL -- bad
    , Flags        XML        NULL -- bad
);
GO

CREATE CLUSTERED INDEX clcs_ArchOrders -- regular index is fine with those types
    ON arch.Orders (SignType, OrderChannel, Flags)
    ON [Dt2RangePS](dt);
GO
