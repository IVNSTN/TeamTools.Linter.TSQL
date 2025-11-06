-- compatibility level min: 110
MERGE acme a
using foo f
on a.id = f.id
when matched then update set date = f.date
output $action, $ROWGUID, ROWGUIDCOL, IDENTITYCOL
into dbo.log(action, rowguid, rcol,  [identitycol]);
