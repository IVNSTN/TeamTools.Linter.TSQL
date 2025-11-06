CREATE TABLE dbo.test (
    a int not null constraint DF_test DEFAULT (0),
    constraint CK_xx check  (a = 1)
)
