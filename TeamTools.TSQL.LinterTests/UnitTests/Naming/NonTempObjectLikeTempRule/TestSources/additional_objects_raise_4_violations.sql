CREATE SCHEMA #tmp;
GO

CREATE SYNONYM #tmp FOR dbo.bar;
GO

CREATE SERVICE #srv ON QUEUE myqueue (some_contract);
GO

CREATE MESSAGE TYPE #temp_msg;
GO
