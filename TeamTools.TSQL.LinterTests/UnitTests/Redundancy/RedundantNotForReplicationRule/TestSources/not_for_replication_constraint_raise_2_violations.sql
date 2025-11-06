CREATE TABLE dbo.foo
(
    usr_name    VARCHAR(20) NOT NULL
    , role_id   INT NOT NULL
    , CONSTRAINT ck_name CHECK NOT FOR REPLICATION (usr_name <> 'admin')
)
GO
ALTER TABLE dbo.foo ADD CONSTRAINT fk_role FOREIGN KEY (role_id)
REFERENCES dbo.bar(role_id) NOT FOR REPLICATION;
GO
