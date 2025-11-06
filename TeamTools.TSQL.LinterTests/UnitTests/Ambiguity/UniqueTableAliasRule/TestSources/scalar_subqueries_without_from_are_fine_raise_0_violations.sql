            SELECT
@usd_price = (SELECT last_price FROM dbo.foo WHERE code = 'USD' AND place_code = 'NYSE')
, @eur_price = (SELECT last_price FROM dbo.bar WHERE code = 'EUR' AND place_code = 'NASDAQ')
