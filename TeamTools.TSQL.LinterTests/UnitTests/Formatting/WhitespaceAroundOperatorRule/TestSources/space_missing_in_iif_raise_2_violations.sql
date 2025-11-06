-- compatibility level min: 110
SELECT @b -=IIF(-3<=4, 0, 1); -- after -= and around <=
