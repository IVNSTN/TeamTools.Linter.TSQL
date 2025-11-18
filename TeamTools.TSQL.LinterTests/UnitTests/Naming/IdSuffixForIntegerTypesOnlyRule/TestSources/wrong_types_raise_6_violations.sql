CREATE TABLE foo
(
    bar_id  BIT         not null identity(1,1)
    , IdFar VARCHAR(10) not null
    , id    UNIQUEIDENTIFIER NOT NULL ROWGUIDCOL
)

DECLARE @foo TABLE
(
    ID    UNIQUEIDENTIFIER NOT NULL ROWGUIDCOL
    , bar_id  BIT         not null identity(1,1)
    , IdFar VARCHAR(10) not null
)
