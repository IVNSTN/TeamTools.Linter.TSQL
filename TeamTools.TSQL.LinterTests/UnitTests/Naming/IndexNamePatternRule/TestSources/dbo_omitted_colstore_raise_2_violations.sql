-- compatibility level min: 110
CREATE TABLE dbo.foo
(
    id           INT          NOT NULL --
    , title      VARCHAR(100) NULL
    , open_date  DATETIME     NOT NULL
    , is_visible BIT          NOT NULL
);
GO

CREATE NONCLUSTERED COLUMNSTORE INDEX CL_foo_title_o18e
    ON zoo.foo (title, open_date, is_visible);
GO

CREATE CLUSTERED COLUMNSTORE INDEX CL_foo
    ON zoo.foo;
GO
