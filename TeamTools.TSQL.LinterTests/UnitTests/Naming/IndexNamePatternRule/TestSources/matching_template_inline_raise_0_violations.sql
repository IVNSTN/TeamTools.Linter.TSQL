-- compatibility level min: 130
CREATE TABLE bar.foo
(
    id           INT          NOT NULL
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
    , INDEX IX_bar_foo_open_date (open_date)
    , INDEX IXF_bar_foo_title (title) WHERE title IS NOT NULL
    , INDEX IU_bar_foo_title_open_date UNIQUE (title, open_date)
    , INDEX IUF_bar_foo_title_o18e UNIQUE (title, open_date, is_visible) WHERE is_visible = 1
);
GO
