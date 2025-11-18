CREATE SYNONYM foo
FOR db.dbo.bar;
GO
CREATE SYNONYM foo
FOR [$(server_var)].db.dbo.bar;
