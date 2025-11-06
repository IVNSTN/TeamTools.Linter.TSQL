SELECT db.dbo.f_today(), f_today()
FROM dbo.foo
WHERE bar = 'far';

DECLARE @war DATE = f_today();

SELECT CASE WHEN [$(db)].dbo.f_today() > GETDATE() THEN 1 END
WHERE [$()].[f_today]() IS NOT NULL

IF DAY([srv].[db].[dbo].[f_today]()) > 12
    RETURN;
