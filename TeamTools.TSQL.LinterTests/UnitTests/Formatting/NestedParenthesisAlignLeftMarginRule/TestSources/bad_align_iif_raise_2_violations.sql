-- compatibility level min: 110
SELECT IIF (
     (select 1) < -- 1
                       (a +
                     (d)  -- 2
                       )
             , 0
             , 1
           )
