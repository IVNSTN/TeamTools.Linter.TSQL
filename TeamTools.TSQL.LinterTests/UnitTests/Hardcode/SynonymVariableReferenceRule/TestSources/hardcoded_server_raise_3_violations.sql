CREATE SYNONYM foo 
FOR server.db.dbo.bar; -- gives 2: for server and db

CREATE SYNONYM foo FOR server.[$(db_var)].dbo.bar;
