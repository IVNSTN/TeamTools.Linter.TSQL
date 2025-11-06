CREATE SYNONYM dbo.frontend_run
FOR [$(PROD2)].[$(frontend)].dbo.run;
