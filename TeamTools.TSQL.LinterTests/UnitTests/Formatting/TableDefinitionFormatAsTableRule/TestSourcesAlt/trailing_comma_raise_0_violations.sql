-- definition is formatted as table nevertheless the comma is trailing
-- another rule must validate comma position in lists
CREATE TABLE #t
(
    id    INT,
    name  VARCHAR,
    dt    DATE,
    PRIMARY KEY (id),
    CONSTRAINT CK CHECK (id > 0),
    CONSTRAINT FK FOREIGN KEY (fk) REFERENCES foo(some_id)
)
