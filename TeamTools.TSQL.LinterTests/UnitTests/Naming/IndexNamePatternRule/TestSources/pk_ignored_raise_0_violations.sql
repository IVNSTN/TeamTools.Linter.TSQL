CREATE TABLE dbo.foo
(
    id INT NOT NULL
    , CONSTRAINT Prmkey_on_id PRIMARY KEY (id)
);
GO

ALTER TABLE dbo.bar
ADD CONSTRAINT PK_dbo_bar_id_fieldNew2 PRIMARY KEY CLUSTERED (id);
GO
