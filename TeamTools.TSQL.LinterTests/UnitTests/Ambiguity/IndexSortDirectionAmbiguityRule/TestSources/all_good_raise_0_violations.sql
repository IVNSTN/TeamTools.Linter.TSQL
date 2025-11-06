CREATE TABLE foo
(
    id int not null primary key,
    b_id int null foreign key references bbb(id),
    c_id int
)

CREATE INDEX idx_foo_bar on foo(c, b, a);

CREATE INDEX idx_foo_bar_zar on foo(c ASC);
CREATE INDEX idx_foo_bar_far on foo(id DESC);
