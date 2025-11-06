CREATE TABLE dbo.foo (id INT, id_mod_2 AS id % 2, name VARCHAR(10));

CREATE TABLE dbo.bar
(
    id         INT         IDENTITY(1, 1) NOT NULL PRIMARY KEY
    , id_div_2 AS (FLOOR(id / 2))
    , name     VARCHAR(10) NOT NULL
    , nm       AS SUBSTRING(name, 1, 5)
    , dt       DATETIME    NOT NULL DEFAULT GETDATE()
    , dt_upd   DATETIME    NULL);
