MERGE trg
USING src
ON src.id = trg.id
WHEN MATCHED AND NOT EXISTS (SELECT trg.UpperLimit, trg.LowerLimit, trg.IdFi INTERSECT SELECT src.UpperLimit, src.LowerLimit, src.IdFi)
                 AND EXISTS (SELECT trg.UpperLimit, trg.LowerLimit, trg.IdFi EXCEPT SELECT src.UpperLimit, src.LowerLimit, src.IdFi) THEN
    DELETE;
