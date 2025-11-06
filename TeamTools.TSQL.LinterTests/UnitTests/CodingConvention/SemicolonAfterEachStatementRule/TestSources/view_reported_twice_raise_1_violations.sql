CREATE VIEW dbo.v_schools
AS
SELECT TOP 100 PERCENT sc.name, sc.id, sc.address, sc.region, sc.map, r.name AS region_name, e.sys_name
FROM  dbo.schools
ORDER BY sc.id
