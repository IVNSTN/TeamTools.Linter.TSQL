-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id INT
    , INDEX IX (id) WITH (IGNORE_DUP_KEY = ON) -- not unique
);
GO
