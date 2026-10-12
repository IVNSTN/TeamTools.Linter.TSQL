CREATE TABLE t
(
    id INT
    , CONSTRAINT PK PRIMARY KEY (id)
    ,   CONSTRAINT CK CHECK (id > 0)
)
GO

CREATE TABLE t
(
    id INT
    , PRIMARY KEY (id)
      , CONSTRAINT FK FOREIGN KEY (fk) REFERENCES foo(some_id)
)
