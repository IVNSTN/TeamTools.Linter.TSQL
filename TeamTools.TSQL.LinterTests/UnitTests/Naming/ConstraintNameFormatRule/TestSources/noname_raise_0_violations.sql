-- missing name is checked by a separate rule
CREATE TABLE dbo.test
(
    a   INT     NOT NULL DEFAULT 0 PRIMARY KEY
    , d INT     CHECK (d <> 0)
    , e VARCHAR CHECK (e = 'e')
);
