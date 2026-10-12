-- compatibility level min: 130
CREATE TABLE dbo.foo
(
    id      INT
    , name  VARCHAR
    , [dt]  DATE
    , calc  AS 1 + 1
    , PRIMARY KEY (id)
    , CONSTRAINT CK CHECK (id > 0)
    , CONSTRAINT FK FOREIGN KEY (fk) REFERENCES foo(some_id)
    , INDEX IX (id)
    , INDEX IY (id DESC)
)
