CREATE TABLE dbo.foo
(
    usr_name    VARCHAR(20) NOT NULL
    , role_id   INT NOT NULL
    , CONSTRAINT ck_name CHECK (usr_name <> 'admin')
)
GO

ALTER TABLE dbo.foo ADD CONSTRAINT fk_role FOREIGN KEY (role_id)
REFERENCES dbo.bar(role_id);
GO

CREATE TRIGGER dbo.my_trg ON dbo.foo AFTER INSERT
AS
BEGIN
    DELETE dbo.bar WHERE id = 1;
END
GO
