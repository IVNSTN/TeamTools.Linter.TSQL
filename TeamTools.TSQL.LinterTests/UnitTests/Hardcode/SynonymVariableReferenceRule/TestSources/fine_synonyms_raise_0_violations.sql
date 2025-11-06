CREATE SYNONYM foo FOR bar;
GO
CREATE SYNONYM foo FOR dbo.bar;
GO
CREATE SYNONYM foo FOR [$(db_var)].dbo.bar;
GO
CREATE SYNONYM foo FOR [$(server_var)].[$(db_var)].dbo.bar;
