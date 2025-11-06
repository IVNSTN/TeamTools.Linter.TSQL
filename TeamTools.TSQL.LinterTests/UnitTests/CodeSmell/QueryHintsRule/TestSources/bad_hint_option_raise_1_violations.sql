-- compatibility level min: 130
SELECT * from A
OPTION (USE HINT ('DISABLE_OPTIMIZER_ROWGOAL'));
