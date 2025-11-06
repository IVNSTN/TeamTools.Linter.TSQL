-- there is a separate rule which forbids naming temp table constraints
CREATE TABLE #test (
    a int not null constraint DF_test DEFAULT (0));

ALTER TABLE #foo
    ADD constraint PK_safd_foo primary key (id);
