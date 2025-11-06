SELECT @bit = IIF(1 = 0, 1, (SELECT TOP (1) id FROM dbo.foo));
