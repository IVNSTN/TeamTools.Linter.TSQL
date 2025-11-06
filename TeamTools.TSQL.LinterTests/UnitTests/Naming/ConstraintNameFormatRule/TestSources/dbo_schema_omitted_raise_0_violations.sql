CREATE TABLE dbo.test2
(
    a INT NOT NULL CONSTRAINT DF_test2_a DEFAULT (0)
    , bbb int
    , CONSTRAINT PK_test2 PRIMARY KEY (id)
    , CONSTRAINT UQ_test2_bbb UNIQUE(bbb)
);
GO

ALTER TABLE foo
ADD CONSTRAINT FK_dbo_foo_type_code_p18d FOREIGN KEY (type_code, parent_id, subtype_id)
    REFERENCES zar.far (code_name, self_id, subtype_id);
GO

ALTER TABLE dbo.foo
ADD CONSTRAINT FK_foo_type_code_p18d FOREIGN KEY (type_code, parent_id, subtype_id)
    REFERENCES zar.far (code_name, self_id, subtype_id);
GO
