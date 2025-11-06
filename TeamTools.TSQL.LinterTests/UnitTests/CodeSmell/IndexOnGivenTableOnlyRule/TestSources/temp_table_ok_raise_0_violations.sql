-- compatibility level min: 130
CREATE table #bar
(
    dummy int not null
    , index ix_d (dummy)
)

create unique nonclustered index ##ix_bar on #bar (dummy);
