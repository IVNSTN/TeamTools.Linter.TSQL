-- compatibility level min: 130
CREATE TABLE #bar (id int, INDEX idx UNIQUE CLUSTERED (id) ON [PRIMARY])
