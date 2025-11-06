-- compatibility level min: 130
DECLARE @foo TABLE
(
    id           INT          NOT NULL
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
    , INDEX IDX_foo_open_date (open_date) -- wrong prefix
    , INDEX IX_foo_title (title) WHERE title IS NOT NULL -- missing F for filtered index
    , INDEX IU_title_open_date UNIQUE (title, open_date) -- no table name
    , INDEX IUF_bar_foo_title_o55e UNIQUE (title, open_date, is_visible) WHERE is_visible = 1 -- wrong numeronim
);
GO
