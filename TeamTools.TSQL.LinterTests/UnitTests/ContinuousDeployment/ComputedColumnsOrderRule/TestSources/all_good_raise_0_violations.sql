DECLARE @bar table
(
    id int,
    id_mod_2 as id % 2,
    name varchar(10));
CREATE TABLE #zar
(
    id int,
    id_div_2 as id % 2,
    name varchar(10));
CREATE TABLE dbo.far
(
    id int,
    name varchar(10) NULL,
    id_div_2 as CAST(id / 2 AS INT)
)
