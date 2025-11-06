;WITH src AS
(
 SELECT
     s.a
 FROM dbo.some_source AS s
 WHERE EXISTS
 (
     SELECT 1
     FROM inserted AS ins
     INNER JOIN deleted AS del
         ON ins.rec_id = del.rec_id
     WHERE ISNULL(ins.abc, 0) <> ISNULL(del.abc, 0)
         AND ins.rec_id = s.rec_id
 )
 GROUP BY s.a
)
            UPDATE t
            SET aaa = src.a + 1
            FROM dbo.upd_target AS t
            INNER JOIN src
            ON src.a = t.a;
